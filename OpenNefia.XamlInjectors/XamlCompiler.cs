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
        public static (bool success, bool writtentofile) Compile(IBuildEngine engine, string input, string[] references,
            string projectDirectory, string output, string strongNameKey, bool debuggerLaunch)
        {
            var typeSystem = new CecilTypeSystem(references
                .Where(r => !r.ToLowerInvariant().EndsWith("opennefia.xamlinjectors.dll"))
                .Concat(new[] { input }), input);

            var asm = typeSystem.TargetAssemblyDefinition;

            if (asm.MainModule.GetType(Constants.CompiledXamlNamespace, "XamlIlContext") != null)
            {
                // If this type exists, the assembly has already been processed by us.
                // Do not run again, it would corrupt the file.
                // This *shouldn't* be possible due to Inputs/Outputs dependencies in the build system,
                // but better safe than sorry eh?
                engine.LogWarningEvent(new BuildWarningEventArgs("XAMLIL", "", "", 0, 0, 0, 0, "Ran twice on same assembly file; ignoring.", "", ""));
                return (true, false);
            }

            var compileRes = CompileCore(engine, typeSystem, debuggerLaunch);
            if (compileRes == null)
                return (true, false);
            if (compileRes == false)
                return (false, false);

            var writerParameters = new WriterParameters { WriteSymbols = asm.MainModule.HasSymbols };
            if (!string.IsNullOrWhiteSpace(strongNameKey))
                writerParameters.StrongNameKeyBlob = File.ReadAllBytes(strongNameKey);

            asm.Write(output, writerParameters);

            return (true, true);

        }

        static bool? CompileCore(IBuildEngine engine, CecilTypeSystem typeSystem, bool debuggerLaunch)
        {
            if (debuggerLaunch && !System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Launch();
            }
            var asm = typeSystem.TargetAssemblyDefinition;
            var embrsc = new EmbeddedResources(asm);

            if (embrsc.Resources.Count(CheckXamlName) == 0)
                // Nothing to do
                return null;

            var xamlLanguage = new XamlLanguageTypeMappings(typeSystem)
            {
                XmlnsAttributes =
                {
                    typeSystem.GetType("Avalonia.Metadata.XmlnsDefinitionAttribute"),

                },
                ContentAttributes =
                {
                    typeSystem.GetType("OpenNefia.Core.UserInterface.XAML.ContentAttribute")
                },
                UsableDuringInitializationAttributes =
                {
                    typeSystem.GetType("OpenNefia.Core.UserInterface.XAML.UsableDuringInitializationAttribute")
                },
                DeferredContentPropertyAttributes =
                {
                    typeSystem.GetType("OpenNefia.Core.UserInterface.XAML.DeferredContentAttribute")
                },
                RootObjectProvider = typeSystem.GetType("OpenNefia.Core.UserInterface.XAML.ITestRootObjectProvider"),
                UriContextProvider = typeSystem.GetType("OpenNefia.Core.UserInterface.XAML.ITestUriContext"),
                ProvideValueTarget = typeSystem.GetType("OpenNefia.Core.UserInterface.XAML.ITestProvideValueTarget"),
            };
            var emitConfig = new XamlLanguageEmitMappings<IXamlILEmitter, XamlILNodeEmitResult>
            {
                ContextTypeBuilderCallback = (b, c) => EmitNameScopeField(xamlLanguage, typeSystem, b, c)
            };

            var transformerconfig = new TransformerConfiguration(
                typeSystem,
                typeSystem.TargetAssembly,
                xamlLanguage,
                XamlXmlnsMappings.Resolve(typeSystem, xamlLanguage), CustomValueConverter);

            var contextDef = new TypeDefinition(Constants.CompiledXamlNamespace, "XamlIlContext",
                TypeAttributes.Class, asm.MainModule.TypeSystem.Object);
            asm.MainModule.Types.Add(contextDef);
            var contextClass = XamlILContextDefinition.GenerateContextClass(typeSystem.CreateTypeBuilder(contextDef), typeSystem,
                xamlLanguage, emitConfig);

            var compiler =
                new OpenNefiaXamlILCompiler(transformerconfig, emitConfig, true);

            var loaderDispatcherDef = new TypeDefinition(Constants.CompiledXamlNamespace, "!XamlLoader",
                TypeAttributes.Class, asm.MainModule.TypeSystem.Object);

            var loaderDispatcherMethod = new MethodDefinition("TryLoad",
                MethodAttributes.Static | MethodAttributes.Public,
                asm.MainModule.TypeSystem.Object)
            {
                Parameters = { new ParameterDefinition(asm.MainModule.TypeSystem.String) }
            };
            loaderDispatcherDef.Methods.Add(loaderDispatcherMethod);
            asm.MainModule.Types.Add(loaderDispatcherDef);

            var stringEquals = asm.MainModule.ImportReference(asm.MainModule.TypeSystem.String.Resolve().Methods.First(
                m =>
                    m.IsStatic && m.Name == "Equals" && m.Parameters.Count == 2 &&
                    m.ReturnType.FullName == "System.Boolean"
                    && m.Parameters[0].ParameterType.FullName == "System.String"
                    && m.Parameters[1].ParameterType.FullName == "System.String"));

            bool CompileGroup(IResourceGroup group)
            {
                var typeDef = new TypeDefinition(Constants.CompiledXamlNamespace, "!" + group.Name, TypeAttributes.Class,
                    asm.MainModule.TypeSystem.Object);

                //typeDef.CustomAttributes.Add(new CustomAttribute(ed));
                asm.MainModule.Types.Add(typeDef);
                var builder = typeSystem.CreateTypeBuilder(typeDef);

                foreach (var res in group.Resources.Where(CheckXamlName))
                {
                    try
                    {
                        engine.LogMessage($"XAMLIL: {res.Name} -> {res.Uri}", MessageImportance.Low);

                        var xaml = new StreamReader(new MemoryStream(res.FileContents)).ReadToEnd();
                        var parsed = XDocumentXamlParser.Parse(xaml);

                        var initialRoot = (XamlAstObjectNode)parsed.Root;

                        var classDirective = initialRoot.Children.OfType<XamlAstXmlDirective>()
                            .FirstOrDefault(d => d.Namespace == XamlNamespaces.Xaml2006 && d.Name == "Class");
                        string classname;
                        if (classDirective != null && classDirective.Values[0] is XamlAstTextNode tn)
                        {
                            classname = tn.Text;
                        }
                        else
                        {
                            classname = res.Name.Replace(".xaml", "");
                        }

                        var classType = typeSystem.TargetAssembly.FindType(classname);
                        if (classType == null)
                            throw new Exception($"Unable to find type '{classname}'");

                        compiler.Transform(parsed);

                        var populateName = $"Populate:{res.Name}";
                        var buildName = $"Build:{res.Name}";

                        var classTypeDefinition = typeSystem.GetTypeReference(classType).Resolve();

                        var populateBuilder = typeSystem.CreateTypeBuilder(classTypeDefinition);

                        compiler.Compile(parsed, contextClass,
                            compiler.DefinePopulateMethod(populateBuilder, parsed, populateName,
                                classTypeDefinition == null),
                            compiler.DefineBuildMethod(builder, parsed, buildName, true),
                            null,
                            null,
                            (closureName, closureBaseType, emitter) =>
                                populateBuilder.DefineSubType(closureBaseType, closureName, false),
                            res.Uri, res
                        );

                        //add compiled populate method
                        var compiledPopulateMethod = typeSystem.GetTypeReference(populateBuilder).Resolve().Methods
                            .First(m => m.Name == populateName);

                        const string TrampolineName = "!XamlIlPopulateTrampoline";
                        var trampoline = new MethodDefinition(TrampolineName,
                            MethodAttributes.Static | MethodAttributes.Private, asm.MainModule.TypeSystem.Void);
                        trampoline.Parameters.Add(new ParameterDefinition(classTypeDefinition));
                        classTypeDefinition.Methods.Add(trampoline);

                        trampoline.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
                        trampoline.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                        trampoline.Body.Instructions.Add(Instruction.Create(OpCodes.Call, compiledPopulateMethod));
                        trampoline.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

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

                        //add compiled build method
                        var compiledBuildMethod = typeSystem.GetTypeReference(builder).Resolve().Methods
                            .First(m => m.Name == buildName);
                        var parameterlessCtor = classTypeDefinition.GetConstructors()
                            .FirstOrDefault(c => c.IsPublic && !c.IsStatic && !c.HasParameters);

                        if (compiledBuildMethod != null && parameterlessCtor != null)
                        {
                            var i = loaderDispatcherMethod.Body.Instructions;
                            var nop = Instruction.Create(OpCodes.Nop);
                            i.Add(Instruction.Create(OpCodes.Ldarg_0));
                            i.Add(Instruction.Create(OpCodes.Ldstr, res.Uri));
                            i.Add(Instruction.Create(OpCodes.Call, stringEquals));
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
                    catch (Exception e)
                    {
                        engine.LogErrorEvent(new BuildErrorEventArgs("XAMLIL", "", res.FilePath, 0, 0, 0, 0,
                            $"{res.FilePath}: {e.Message}", "", "CompileOpenNefiaXaml"));
                    }
                }
                return true;
            }

            if (embrsc.Resources.Count(CheckXamlName) != 0)
            {
                if (!CompileGroup(embrsc))
                    return false;
            }

            loaderDispatcherMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
            loaderDispatcherMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            return true;
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

        private static void EmitNameScopeField(XamlLanguageTypeMappings xamlLanguage, CecilTypeSystem typeSystem, IXamlTypeBuilder<IXamlILEmitter> typeBuilder, IXamlILEmitter constructor)
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

    interface IResource : IFileSource
    {
        string Uri { get; }
        string Name { get; }
        void Remove();

    }

    interface IResourceGroup
    {
        string Name { get; }
        IEnumerable<IResource> Resources { get; }
    }
}
