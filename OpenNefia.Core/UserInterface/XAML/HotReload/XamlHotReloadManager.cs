﻿using Avalonia.Metadata;
using HarmonyLib;
using Mono.Reflection;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.Utility;
using OpenNefia.XamlInjectors.CompilerExtensions;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using XamlX.Ast;
using XamlX.Emit;
using XamlX.IL;
using XamlX.Parsers;
using XamlX.Transform;
using XamlX.TypeSystem;
using Sre = System.Reflection.Emit;
using static OpenNefia.XamlInjectors.XamlCompiler;
using JetBrains.Annotations;
using OpenNefia.Core.Log;
using OpenNefia.XamlInjectors;
using OpenNefia.Core.IoC;
using OpenNefia.Core.HotReload;

namespace OpenNefia.Core.UserInterface.XAML.HotReload
{
    internal interface IXamlHotReloadManager
    {
        void Initialize();
        bool IsXamlCompiledControlType(Type type);
        bool HotReloadXamlControl(Type controlType, string xamlPath);
    }

    internal sealed class XamlHotReloadManager : IXamlHotReloadManager
    {
        [Dependency] private readonly IHotReloadWatcher _hotReloadWatcher = default!;

        private Harmony _harmony;
        private SreTypeSystem _typeSystem = default!;
        private XamlLanguageTypeMappings _xamlLanguage = default!;
        private XamlXmlnsMappings _xmlnsMappings = default!;
        private XamlLanguageEmitMappings<IXamlILEmitter, XamlILNodeEmitResult> _emitConfig = default!;

        public XamlHotReloadManager()
        {
            _harmony = new Harmony("io.opennefia.wisp.hotreload");
        }

        public void Initialize()
        {
            _typeSystem = new SreTypeSystem();

            _xamlLanguage = new XamlLanguageTypeMappings(_typeSystem)
            {
                XmlnsAttributes =
                {
                    _typeSystem.GetType(typeof(XmlnsDefinitionAttribute)),
                },
                ContentAttributes =
                {
                    _typeSystem.GetType(typeof(ContentAttribute))
                },
                UsableDuringInitializationAttributes =
                {
                    _typeSystem.GetType(typeof(UsableDuringInitializationAttribute))
                },
                DeferredContentPropertyAttributes =
                {
                    _typeSystem.GetType(typeof(DeferredContentAttribute))
                },
            };

            _xmlnsMappings = XamlXmlnsMappings.Resolve(_typeSystem, _xamlLanguage);

            _emitConfig = new XamlLanguageEmitMappings<IXamlILEmitter, XamlILNodeEmitResult>
            {
                ContextTypeBuilderCallback = (b, c) => EmitNameScopeField(_xamlLanguage, _typeSystem, b, c)
            };

            _hotReloadWatcher.OnUpdateApplication += OnHotReload;
        }

        private void OnHotReload(HotReloadEventArgs args)
        {
            if (args.UpdatedTypes != null)
            {
                foreach (var type in args.UpdatedTypes)
                {
                    if (IsXamlCompiledControlType(type))
                    {
                        Logger.InfoS("wisp.hotreload", $"Detected hot reload of XAML-compiled class: {type}");
                        PatchLoadXamlCalls(type);
                    }
                }
            }
        }

        public bool IsXamlCompiledControlType(Type type)
        {
            return typeof(WispControl).IsAssignableFrom(type) 
                && type.GetMethod(XamlCompiler.TrampolineName, BindingFlags.Static | BindingFlags.NonPublic) != null;
        }

        /// <summary>
        /// Gets the assembly references necessary to compile XAML for this assembly.
        /// These should have been already generated by the OpenNefia.XamlInjectors task.
        /// </summary>
        private string[] GetAssemblyReferences(Assembly assembly)
        {
            // TODO implement
            return File.ReadAllLines("C:/Users/yuno/build/OpenNefia.NET/OpenNefia.Core/obj/Debug/net6.0/XAML/references");
        }

        public bool HotReloadXamlControl(Type controlType, string xamlPath)
        {
            if (!typeof(WispControl).IsAssignableFrom(controlType))
            {
                throw new InvalidOperationException($"Type '{controlType}' does not inherit from {nameof(WispControl)}.");
            }

            if (!IsXamlCompiledControlType(controlType))
            {
                throw new InvalidOperationException($"Control type '{controlType}' was not generated from XAML (missing trampoline)");
            }

            if (!File.Exists(xamlPath))
            {
                throw new FileNotFoundException($"XAML file for {controlType} not found: {xamlPath}");
            }

            var containingAssembly = controlType.Assembly;
            var assemblyReferences = GetAssemblyReferences(containingAssembly);
            var xamlResource = new XamlResource(controlType, xamlPath);

            var transformerConfig = new TransformerConfiguration(
            _typeSystem,
            _typeSystem.GetAssembly(containingAssembly),
            _xamlLanguage,
            _xmlnsMappings,
            CustomValueConverter);

            var compiler = new OpenNefiaXamlILCompiler(transformerConfig, _emitConfig, true);

            XamlDocument parsed;
            using (var stream = new MemoryStream(xamlResource.FileContents))
            {
                using (var reader = new StreamReader(stream))
                {
                    var xaml = reader.ReadToEnd();
                    parsed = XDocumentXamlParser.Parse(xaml);
                }
            }

            var xamlClassType = GetClassTypeFromXaml(xamlResource, parsed);

            if (xamlClassType.FullName != controlType.FullName)
            {
                throw new InvalidDataException($"Class type mismatch: passed={controlType.FullName}, inXaml={xamlClassType.FullName}");
            }

            compiler.Transform(parsed);

            var populateMethod = CompileNewPopulateMethod(compiler, xamlResource, parsed);
            var newIl = GetCodeInstructions(populateMethod);
            UpdatePopulateMethodIL(controlType, populateMethod, newIl);

            Logger.InfoS("wisp.hotreload", $"Hot reloaded XAML of {controlType}.");

            return true;
        }

        #region XAML Compilation

        /// <summary>
        /// Compiles the new XAML to a method body holding the new control initialization code.
        /// </summary>
        private MethodInfo CompileNewPopulateMethod(OpenNefiaXamlILCompiler compiler, IResource xamlResource, XamlDocument parsed)
        {
            // Create a dummy module to hold the new populate method.
            var module = AssemblyBuilder
                .DefineDynamicAssembly(
                    new AssemblyName($"{nameof(XamlHotReloadManager)}_Assembly"),
                    AssemblyBuilderAccess.RunAndCollect)
                .DefineDynamicModule($"{nameof(XamlHotReloadManager)}_Module");

            // First define the "context" type, which the XamlX compiler requires.
            var sreContextTypeBuilder = module
                .DefineType("XamlIlContext",
                    TypeAttributes.Class | TypeAttributes.Public);
            var xamlContextTypeBuilder = _typeSystem.CreateTypeBuilder(sreContextTypeBuilder);
            var contextClass = XamlILContextDefinition.GenerateContextClass(xamlContextTypeBuilder, _typeSystem, _xamlLanguage, _emitConfig);

            // Next, define a dummy type that will hold the new body of the autogenerated
            // "populate" method that initializes controls and set properties, etc.
            // This method body is what we're going to replace the current class's with.
            var sreDummyTypeBuilder = module
                .DefineType(
                    "PopulateMethodHolderDummy",
                    TypeAttributes.Class | TypeAttributes.Public);
            var xamlDummyTypeBuilder = _typeSystem.CreateTypeBuilder(sreDummyTypeBuilder);

            // This method name is the same as the one generated by OpenNefia.XamlInjectors.
            var populateName = $"Populate:{xamlResource.Name}";
            var populateMethod = compiler.DefinePopulateMethod(xamlDummyTypeBuilder, parsed, populateName, isPublic: false);

            compiler.Compile(parsed, contextClass,
                populateMethod,
                buildMethod: null,
                namespaceInfoBuilder: null,
                createClosure: null,
                createDelegateType: (closureName, closureBaseType, emitter) =>
                    xamlContextTypeBuilder.DefineSubType(closureBaseType, closureName, false),
                xamlResource.Uri,
                xamlResource
            );

            var compiledPopulateMethod = sreDummyTypeBuilder.CreateTypeInfo()!.DeclaredMethods
                .Where(m => m.Name == populateName).First();

            return compiledPopulateMethod;
        }

        private IXamlType GetClassTypeFromXaml(IResource res, XamlDocument parsed)
        {
            var classname = XamlAstHelpers.GetClassNameFromXaml(res.Name, parsed);

            var classType = _typeSystem.FindType(classname);
            if (classType == null)
                throw new Exception($"Unable to find type '{classname}'");
            return classType;
        }

        private static bool CustomValueConverter(
            AstTransformationContext context,
            IXamlAstValueNode node,
            IXamlType type,
            [NotNullWhen(true)] out IXamlAstValueNode? result)
        {
            if (!(node is XamlAstTextNode textNode))
            {
                result = null;
                return false;
            }

            var text = textNode.Text;
            var types = context.GetOpenNefiaTypes();

            if (Parsing.TryConvert(context, node, text, type, types, out result))
            {
                return true;
            }

            result = null;
            return false;
        }

        #endregion

        #region IL Patching

        private static Sre.Label MakeLabel(int labelIndex)
        {
            var constructor = typeof(Sre.Label).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, new Type[] { typeof(int) }, null)!;

            return (Sre.Label)constructor.Invoke(new object[] { labelIndex });
        }

        /// <summary>
        /// Converts a method body into a Harmony-compatible bytecode format.
        /// </summary>
        private List<CodeInstruction> GetCodeInstructions(MethodInfo method)
        {
            var monoInstructions = method.GetInstructions();
            var harmonyInstructions = ConvertMonoILToHarmonyIL(monoInstructions);

            return harmonyInstructions;
        }

        private List<CodeInstruction> ConvertMonoILToHarmonyIL(IList<Instruction> monoInstructions)
        {
            var harmonyInstructions = monoInstructions.Select(i => new CodeInstruction(i.OpCode, i.Operand)).ToList();
            ConvertLabels(monoInstructions, harmonyInstructions);
            return harmonyInstructions;
        }

        /// <summary>
        /// Converts IL labels from Mono.Reflection to System.Reflection.Emit.
        /// </summary>
        private static void ConvertLabels(IList<Instruction> monoInstructions, List<CodeInstruction> harmonyInstructions)
        {
            var lookup = new Dictionary<int, int>();
            var i = 0;
            foreach (var inst in monoInstructions)
            {
                lookup[inst.Offset] = i;
            }

            var labelLookup = new Dictionary<int, Label>();
            var labelIndex = 0;
            foreach (var instruction in harmonyInstructions)
            {
                // Check for Mono.Reflection instruction references and convert them to SRE labels.
                if (instruction.operand is Mono.Reflection.Instruction monoInst)
                {
                    var label = labelLookup.GetValueOrInsert(monoInst.Offset, () =>
                    {
                        var l = MakeLabel(labelIndex);
                        labelIndex += 1;
                        return l;
                    });

                    var harmonyInst = harmonyInstructions[lookup[monoInst.Offset]];
                    harmonyInst.labels.Add(label);
                    instruction.operand = label;
                }
            }
        }

        /// <summary>
        /// Runs Harmony to patch the XAML populate method on the control.
        /// </summary>
        private void UpdatePopulateMethodIL(Type controlType, MethodInfo populateMethod, List<CodeInstruction> newMethodBody)
        {
            MethodBodyPatch.NewMethodBody = newMethodBody;

            var original = controlType.GetMethod(populateMethod.Name, BindingFlags.NonPublic | BindingFlags.Static);

            _harmony.Patch(original, transpiler: MethodBodyPatch.TranspilerMethod);
        }

        /// <summary>
        /// Replaces calls to <see cref="OpenNefiaXamlLoader.Load(object)"/> with the call
        /// to the populate method. Is functionally the same as the version in the compiler
        /// task, except this version operates at runtime.
        /// </summary>
        private void PatchLoadXamlCalls(Type controlType)
        {
            if (!typeof(WispControl).IsAssignableFrom(controlType))
            {
                throw new InvalidOperationException($"Type '{controlType}' does not inherit from {nameof(WispControl)}.");
            }

            var trampoline = controlType.GetMethod(XamlCompiler.TrampolineName, BindingFlags.Static | BindingFlags.NonPublic);

            if (trampoline == null)
            {
                throw new ArgumentException($"Type '{controlType}' was not compiled from XAML. (missing trampoline)", nameof(controlType));
            }

            var furthestPartialClass = trampoline.DeclaringType!;

            // Methods currently aren't scanned due to performance reasons. The call to
            // OpenNefiaXamlLoader.Load(this) has to be in a constructor if the control
            // is to be hot reload compatible.
            IEnumerable<MethodBase> methods = controlType.GetConstructors()
                .Where(m => !m.Attributes.HasFlag(MethodAttributes.Static) && m.HasMethodBody());

            XamlLoadPatch.Trampoline = trampoline;
            XamlLoadPatch.FoundXamlLoader = false;

            // Find OpenNefiaXamlLoader.Load(this) and replace it with !XamlIlPopulateTrampoline(this)
            foreach (var method in methods)
            {
                _harmony.Patch(method.GetDeclaredMember(), transpiler: XamlLoadPatch.TranspilerMethod);
            }

            if (!XamlLoadPatch.FoundXamlLoader)
            {
                var ctors = controlType.GetConstructors().Where(c => !c.IsStatic).ToList();
                // We can inject xaml loader into default constructor
                if (ctors.Count == 1 && ctors[0].GetInstructions().Count(o => o.OpCode != OpCodes.Nop) == 3)
                {
                    Logger.DebugS("wisp.hotreload", $"Injecting trampoline into parameterless constructor.");

                    ConstructorXamlLoadPatch.Trampoline = trampoline;
                    _harmony.Patch(ctors[0].GetDeclaredMember(), transpiler: ConstructorXamlLoadPatch.TranspilerMethod);
                }
                else
                {
                    throw new InvalidProgramException(
                        $"No call to {nameof(OpenNefiaXamlLoader)}.Load(this) call found anywhere in the type {controlType.FullName} and type seems to have custom constructors.");
                }
            }
        }

        /// <summary>
        /// Transpiler class used by Harmony.
        /// </summary>
        private static class MethodBodyPatch
        {
            public static IEnumerable<CodeInstruction> NewMethodBody { get; set; } = default!;

            public static HarmonyMethod TranspilerMethod =>
                new HarmonyMethod(typeof(MethodBodyPatch)
                    .GetMethod(nameof(Transpiler), BindingFlags.NonPublic | BindingFlags.Static));

            [UsedImplicitly]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return NewMethodBody!;
            }
        }

        /// <summary>
        /// Transpiler class used by Harmony.
        /// </summary>
        private static class XamlLoadPatch
        {
            public static MethodInfo Trampoline { get; set; } = default!;
            public static bool FoundXamlLoader { get; set; }

            public static HarmonyMethod TranspilerMethod =>
                new HarmonyMethod(typeof(XamlLoadPatch)
                    .GetMethod(nameof(Transpiler), BindingFlags.NonPublic | BindingFlags.Static));

            [UsedImplicitly]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var i = instructions.ToList();
                for (var c = 1; c < i.Count; c++)
                {
                    if (i[c].opcode == OpCodes.Call)
                    {
                        var op = i[c].operand as MethodInfo;

                        if (op != null && op.Name == "Load")
                        {
                            var parameters = op.GetParameters();
                            if (parameters.Length == 1
                                && parameters[0].ParameterType == typeof(object)
                                && op.DeclaringType == typeof(OpenNefiaXamlLoader))
                            {
                                if (MatchThisCall(i, c - 1))
                                {
                                    Logger.DebugS("wisp.hotreload", $"Found call to {nameof(OpenNefiaXamlLoader)}.Load(this), index={i}.");

                                    i[c].operand = Trampoline;
                                    FoundXamlLoader = true;
                                }
                            }
                        }
                    }
                }

                return i;
            }

            private static bool MatchThisCall(IList<CodeInstruction> instructions, int idx)
            {
                var i = instructions[idx];
                // A "normal" way of passing `this` to a static method:

                // ldarg.0
                // call void [Avalonia.Markup.Xaml]Avalonia.Markup.Xaml.AvaloniaXamlLoader::Load(object)

                return i.opcode == OpCodes.Ldarg_0 || (i.opcode == OpCodes.Ldarg && i.operand?.Equals(0) == true);
            }
        }

        /// <summary>
        /// Transpiler class used by Harmony.
        /// </summary>
        private static class ConstructorXamlLoadPatch
        {
            public static MethodInfo Trampoline { get; set; } = default!;

            public static HarmonyMethod TranspilerMethod =>
                new HarmonyMethod(typeof(ConstructorXamlLoadPatch)
                    .GetMethod(nameof(Transpiler), BindingFlags.NonPublic | BindingFlags.Static));

            [UsedImplicitly]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var i = instructions.ToList();
                var retIdx = i.IndexOf(i.Last(x => x.opcode == OpCodes.Ret));
                i.Insert(retIdx, new CodeInstruction(OpCodes.Call, Trampoline));
                i.Insert(retIdx, new CodeInstruction(OpCodes.Ldarg_0));
                return i;
            }
        }

        #endregion

    }
}