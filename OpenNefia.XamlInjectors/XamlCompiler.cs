using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using OpenNefia.XamlInjectors.CompilerExtensions;
using XamlX;
using XamlX.Ast;
using XamlX.Emit;
using XamlX.IL;
using XamlX.Parsers;
using XamlX.Transform;
using XamlX.TypeSystem;

namespace OpenNefia.XamlInjectors
{
    /// <summary>
    /// Based on https://github.com/AvaloniaUI/Avalonia/blob/c85fa2b9977d251a31886c2534613b4730fbaeaf/src/Avalonia.Build.Tasks/XamlCompilerTaskExecutor.cs
    /// Adjusted for our UI-Framework
    /// </summary>
    public partial class XamlCompiler
    {
        public IBuildEngine? Engine { get; }
        public CecilTypeSystem TypeSystem { get; }

        public XamlLanguageTypeMappings XamlLanguage { get; }
        public XamlLanguageEmitMappings<IXamlILEmitter, XamlILNodeEmitResult> EmitConfig { get; }
        public TransformerConfiguration TransformerConfig { get; }

        public OpenNefiaXamlILCompiler Compiler { get; }

        public AssemblyDefinition TargetAssembly => TypeSystem.TargetAssemblyDefinition;
        public ModuleDefinition MainModule => TargetAssembly.MainModule;
        public MethodReference StringEquals { get; }

        public XamlCompiler(string input, string[] references, IBuildEngine? engine = null)
        {
            Engine = engine;
            TypeSystem = new CecilTypeSystem(references
                .Where(r => !r.ToLowerInvariant().EndsWith("opennefia.xamlinjectors.dll"))
                .Concat(new[] { input }), input);

            XamlLanguage = new XamlLanguageTypeMappings(TypeSystem)
            {
                XmlnsAttributes =
                {
                    TypeSystem.GetType("Avalonia.Metadata.XmlnsDefinitionAttribute"),

                },
                ContentAttributes =
                {
                    TypeSystem.GetType("OpenNefia.Core.UserInterface.XAML.ContentAttribute")
                },
                UsableDuringInitializationAttributes =
                {
                    TypeSystem.GetType("OpenNefia.Core.UserInterface.XAML.UsableDuringInitializationAttribute")
                },
                DeferredContentPropertyAttributes =
                {
                    TypeSystem.GetType("OpenNefia.Core.UserInterface.XAML.DeferredContentAttribute")
                },
                RootObjectProvider = TypeSystem.GetType("OpenNefia.Core.UserInterface.XAML.ITestRootObjectProvider"),
                UriContextProvider = TypeSystem.GetType("OpenNefia.Core.UserInterface.XAML.ITestUriContext"),
                ProvideValueTarget = TypeSystem.GetType("OpenNefia.Core.UserInterface.XAML.ITestProvideValueTarget"),
            };

            EmitConfig = new XamlLanguageEmitMappings<IXamlILEmitter, XamlILNodeEmitResult>
            {
                ContextTypeBuilderCallback = (b, c) => EmitNameScopeField(XamlLanguage, TypeSystem, b, c)
            };

            TransformerConfig = new TransformerConfiguration(
                TypeSystem,
                TypeSystem.TargetAssembly,
                XamlLanguage,
                XamlXmlnsMappings.Resolve(TypeSystem, XamlLanguage), CustomValueConverter);

            Compiler =
                new OpenNefiaXamlILCompiler(TransformerConfig, EmitConfig, true);

            StringEquals = MainModule.ImportReference(MainModule.TypeSystem.String.Resolve().Methods.First(
                m =>
                    m.IsStatic && m.Name == "Equals" && m.Parameters.Count == 2 &&
                    m.ReturnType.FullName == "System.Boolean"
                    && m.Parameters[0].ParameterType.FullName == "System.String"
                    && m.Parameters[1].ParameterType.FullName == "System.String"));
        }

        public (bool success, bool writtentofile) Compile(string output, string strongNameKey, bool debuggerLaunch)
        {
            if (TargetAssembly.MainModule.GetType(Constants.CompiledXamlNamespace, "XamlIlContext") != null)
            {
                // If this type exists, the assembly has already been processed by us.
                // Do not run again, it would corrupt the file.
                // This *shouldn't* be possible due to Inputs/Outputs dependencies in the build system,
                // but better safe than sorry eh?
                Engine?.LogWarningEvent(new BuildWarningEventArgs("XAMLIL", "", "", 0, 0, 0, 0, "Ran twice on same assembly file; ignoring.", "", ""));
                return (true, false);
            }

            var compileRes = CompileCore(debuggerLaunch);
            if (compileRes == null)
                return (true, false);
            if (compileRes == false)
                return (false, false);

            var writerParameters = new WriterParameters { WriteSymbols = MainModule.HasSymbols };
            if (!string.IsNullOrWhiteSpace(strongNameKey))
                writerParameters.StrongNameKeyBlob = File.ReadAllBytes(strongNameKey);

            TargetAssembly.Write(output, writerParameters);

            return (true, true);

        }

        private const string TrampolineName = "!XamlIlPopulateTrampoline";

        public class CompileGroupResult
        {
            public CompileGroupResult(IXamlType classType,
                TypeDefinition classTypeDefinition,
                MethodDefinition compiledPopulateMethod,
                MethodDefinition compiledBuildMethod,
                MethodDefinition parameterlessCtor,
                IResource resource)
            {
                ClassType = classType;
                ClassTypeDefinition = classTypeDefinition;
                CompiledBuildMethod = compiledBuildMethod;
                CompiledPopulateMethod = compiledPopulateMethod;
                ParameterlessCtor = parameterlessCtor;
                Resource = resource;
            }

            public IXamlType ClassType { get; }
            public TypeDefinition ClassTypeDefinition { get; }
            public MethodDefinition CompiledBuildMethod { get; }
            public MethodDefinition CompiledPopulateMethod { get; }
            public MethodDefinition ParameterlessCtor { get; }
            public IResource Resource { get; }
        }

        private bool? CompileCore(bool debuggerLaunch)
        {
            if (debuggerLaunch && !System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Launch();
            }

            var resourceGroup = new EmbeddedResources(TargetAssembly);
            if (resourceGroup.Resources.Count(CheckXamlName) == 0)
                // Nothing to do
                return null;

            var contextDef = new TypeDefinition(Constants.CompiledXamlNamespace, "XamlIlContext",
                TypeAttributes.Class, MainModule.TypeSystem.Object);
            MainModule.Types.Add(contextDef);

            var contextClass = XamlILContextDefinition.GenerateContextClass(TypeSystem.CreateTypeBuilder(contextDef), TypeSystem,
                XamlLanguage, EmitConfig);

            var results = new List<CompileGroupResult>();

            if (resourceGroup.Resources.Count(CheckXamlName) != 0)
            {
                var typeDef = new TypeDefinition(Constants.CompiledXamlNamespace, "!" + resourceGroup.Name, TypeAttributes.Class,
                    TargetAssembly.MainModule.TypeSystem.Object);

                //typeDef.CustomAttributes.Add(new CustomAttribute(ed));
                TargetAssembly.MainModule.Types.Add(typeDef);
                var builder = TypeSystem.CreateTypeBuilder(typeDef);

                results = CompileGroup(resourceGroup, contextClass, builder);

                foreach (var result in results)
                {
                    AddInitializer(result);
                }
            }

            AddLoaderMethod(results);

            return true;
        }

        private List<CompileGroupResult> CompileGroup(IResourceGroup group, IXamlType contextClass, IXamlTypeBuilder<IXamlILEmitter> builder)
        {
            var results = new List<CompileGroupResult>();

            foreach (var res in group.Resources.Where(CheckXamlName))
            {
                try
                {
                    results.Add(CompileSingleResource(res, contextClass, builder));
                }
                catch (Exception e)
                {
                    Engine?.LogErrorEvent(new BuildErrorEventArgs("XAMLIL", "", res.FilePath, 0, 0, 0, 0,
                        $"{res.FilePath}: {e.Message}", "", "CompileOpenNefiaXaml"));
                }
            }
            return results;
        }

        public CompileGroupResult CompileSingleResource(IResource res, IXamlType contextClass, IXamlTypeBuilder<IXamlILEmitter>? builder = null)
        {
            Engine?.LogMessage($"XAMLIL: {res.Name} -> {res.Uri}", MessageImportance.Low);

            var xaml = new StreamReader(new MemoryStream(res.FileContents)).ReadToEnd();
            var parsed = XDocumentXamlParser.Parse(xaml);

            var classType = GetClassTypeFromXaml(res, parsed);

            Compiler.Transform(parsed);

            var populateName = $"Populate:{res.Name}";
            var buildName = $"Build:{res.Name}";

            var classTypeDefinition = TypeSystem.GetTypeReference(classType).Resolve();

            var populateBuilder = TypeSystem.CreateTypeBuilder(classTypeDefinition);

            Compiler.Compile(parsed, contextClass,
                Compiler.DefinePopulateMethod(populateBuilder, parsed, populateName,
                    classTypeDefinition == null),
                builder != null ? Compiler.DefineBuildMethod(builder, parsed, buildName, true) : null,
                null,
                null,
                (closureName, closureBaseType, emitter) =>
                    populateBuilder.DefineSubType(closureBaseType, closureName, false),
                res.Uri, res
            );

            //add compiled populate method
            var compiledPopulateMethod = TypeSystem.GetTypeReference(populateBuilder).Resolve().Methods
                .First(m => m.Name == populateName);

            MethodDefinition? compiledBuildMethod = null;

            if (builder != null)
            {
                //add compiled build method
                compiledBuildMethod = TypeSystem.GetTypeReference(builder).Resolve().Methods
                    .First(m => m.Name == buildName);
            }

            var parameterlessCtor = classTypeDefinition.GetConstructors()
                .FirstOrDefault(c => c.IsPublic && !c.IsStatic && !c.HasParameters);

            return new CompileGroupResult(classType, classTypeDefinition!, compiledPopulateMethod, compiledBuildMethod, parameterlessCtor, res);
        }

        private IXamlType GetClassTypeFromXaml(IResource res, XamlDocument parsed)
        {
            var classname = XamlAstHelpers.GetClassNameFromXaml(res.FilePath, parsed);

            var classType = TypeSystem.TargetAssembly.FindType(classname);
            if (classType == null)
                throw new Exception($"Unable to find type '{classname}'");
            return classType;
        }

        private void AddInitializer(CompileGroupResult result)
        {
            var classTypeDefinition = result.ClassTypeDefinition;
            var compiledPopulateMethod = result.CompiledPopulateMethod;

            var trampoline = AddTrampoline(TargetAssembly, classTypeDefinition, compiledPopulateMethod);
            ReplaceLoadXamlCalls(result.ClassType, classTypeDefinition, trampoline);
        }

        private void AddLoaderMethod(List<CompileGroupResult> results)
        {
            var loaderDispatcherDef = new TypeDefinition(Constants.CompiledXamlNamespace, "!XamlLoader",
                TypeAttributes.Class, MainModule.TypeSystem.Object);

            var loaderDispatcherMethod = new MethodDefinition("TryLoad",
                MethodAttributes.Static | MethodAttributes.Public,
                MainModule.TypeSystem.Object)
            {
                Parameters = { new ParameterDefinition(MainModule.TypeSystem.String) }
            };
            loaderDispatcherDef.Methods.Add(loaderDispatcherMethod);
            MainModule.Types.Add(loaderDispatcherDef);

            foreach (var result in results)
            {
                AddBuildMethodToLoader(loaderDispatcherMethod, result);
            }

            loaderDispatcherMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
            loaderDispatcherMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private void AddBuildMethodToLoader(MethodDefinition loaderDispatcherMethod, CompileGroupResult result)
        {
            var compiledBuildMethod = result.CompiledBuildMethod;
            var parameterlessCtor = result.ParameterlessCtor;

            if (compiledBuildMethod != null && parameterlessCtor != null)
            {
                var i = loaderDispatcherMethod.Body.Instructions;
                var nop = Instruction.Create(OpCodes.Nop);
                i.Add(Instruction.Create(OpCodes.Ldarg_0));
                i.Add(Instruction.Create(OpCodes.Ldstr, result.Resource.Uri));
                i.Add(Instruction.Create(OpCodes.Call, StringEquals));
                i.Add(Instruction.Create(OpCodes.Brfalse, nop));
                if (parameterlessCtor != null)
                    i.Add(Instruction.Create(OpCodes.Newobj, parameterlessCtor));
                else
                {
                    i.Add(Instruction.Create(OpCodes.Call, compiledBuildMethod));
                }

                i.Add(Instruction.Create(OpCodes.Ret));
                i.Add(nop);
            }
        }

        private MethodDefinition AddTrampoline(AssemblyDefinition asm, TypeDefinition classTypeDefinition, MethodDefinition compiledPopulateMethod)
        {
            var trampoline = new MethodDefinition(TrampolineName,
                                        MethodAttributes.Static | MethodAttributes.Private, MainModule.TypeSystem.Void);
            trampoline.Parameters.Add(new ParameterDefinition(classTypeDefinition));
            classTypeDefinition.Methods.Add(trampoline);

            trampoline.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
            trampoline.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            trampoline.Body.Instructions.Add(Instruction.Create(OpCodes.Call, compiledPopulateMethod));
            trampoline.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            return trampoline;
        }

        private static void ReplaceLoadXamlCalls(IXamlType classType, TypeDefinition classTypeDefinition, MethodDefinition trampoline)
        {
            var foundXamlLoader = false;
            // Find OpenNefiaXamlLoader.Load(this) and replace it with !XamlIlPopulateTrampoline(this)
            foreach (var method in classTypeDefinition.Methods
                .Where(m => !m.Attributes.HasFlag(MethodAttributes.Static)))
            {
                var i = method.Body.Instructions;
                for (var c = 1; c < i.Count; c++)
                {
                    if (i[c].OpCode == OpCodes.Call)
                    {
                        var op = i[c].Operand as MethodReference;

                        if (op != null
                            && op.Name == TrampolineName)
                        {
                            foundXamlLoader = true;
                            break;
                        }

                        if (op != null
                            && op.Name == "Load"
                            && op.Parameters.Count == 1
                            && op.Parameters[0].ParameterType.FullName == "System.Object"
                            && op.DeclaringType.FullName == "OpenNefia.Core.UserInterface.XAML.OpenNefiaXamlLoader")
                        {
                            if (MatchThisCall(i, c - 1))
                            {
                                i[c].Operand = trampoline;
                                foundXamlLoader = true;
                            }
                        }
                    }
                }
            }

            if (!foundXamlLoader)
            {
                var ctors = classTypeDefinition.GetConstructors()
                    .Where(c => !c.IsStatic).ToList();
                // We can inject xaml loader into default constructor
                if (ctors.Count == 1 && ctors[0].Body.Instructions.Count(o => o.OpCode != OpCodes.Nop) == 3)
                {
                    var i = ctors[0].Body.Instructions;
                    var retIdx = i.IndexOf(i.Last(x => x.OpCode == OpCodes.Ret));
                    i.Insert(retIdx, Instruction.Create(OpCodes.Call, trampoline));
                    i.Insert(retIdx, Instruction.Create(OpCodes.Ldarg_0));
                }
                else
                {
                    throw new InvalidProgramException(
                        $"No call to OpenNefiaXamlLoader.Load(this) call found anywhere in the type {classType.FullName} and type seems to have custom constructors.");
                }
            }
        }

        private static bool CustomValueConverter(
            AstTransformationContext context,
            IXamlAstValueNode node,
            IXamlType type,
            out IXamlAstValueNode result)
        {
            if (!(node is XamlAstTextNode textNode))
            {
                result = null;
                return false;
            }

            var text = textNode.Text;
            var types = context.GetOpenNefiaTypes();

            if (ONXamlParseIntrinsics.TryConvert(context, node, text, type, types, out result))
            {
                return true;
            }

            result = null;
            return false;
        }

        public const string ContextNameScopeFieldName = "OpenNefiaNameScope";

        public static void EmitNameScopeField(XamlLanguageTypeMappings xamlLanguage, IXamlTypeSystem typeSystem, IXamlTypeBuilder<IXamlILEmitter> typeBuilder, IXamlILEmitter constructor)
        {
            var nameScopeType = typeSystem.FindType(Constants.NameScopeTypeName);
            var field = typeBuilder.DefineField(nameScopeType,
                ContextNameScopeFieldName, true, false);
            constructor
                .Ldarg_0()
                .Newobj(nameScopeType.GetConstructor())
                .Stfld(field);
        }
    }
}
