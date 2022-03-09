using Avalonia.Metadata;
using CommandLine;
using HarmonyLib;
using JetBrains.Annotations;
using Microsoft.Build.Framework;
using Mono.Cecil.Cil;
using OpenNefia.Core.Log;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.UserInterface.XAML;
using OpenNefia.XamlInjectors;
using OpenNefia.XamlInjectors.CompilerExtensions;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Linq;
using XamlX;
using XamlX.Ast;
using XamlX.Emit;
using XamlX.IL;
using XamlX.Parsers;
using XamlX.Transform;
using XamlX.TypeSystem;
using Mono.Reflection;
using static OpenNefia.XamlInjectors.XamlCompiler;
using Sre = System.Reflection.Emit;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.ControlTest
{
    public partial class ControlDebugWindow : DefaultWindow
    {
        public ControlDebugWindow()
        {
            OpenNefiaXamlLoader.Load(this);

            ToggleDebugButton.OnPressed += ToggleDebug;
            PatchButton.OnPressed += DoPatch;
        }

        private void ToggleDebug(BaseButton.ButtonEventArgs obj)
        {
            WispRootLayer!.Debug = !WispRootLayer.Debug;
        }

        private void DoPatch(BaseButton.ButtonEventArgs obj)
        {
            var asm = typeof(Engine).Assembly;

            var references = File.ReadAllLines("C:/Users/yuno/build/OpenNefia.NET/OpenNefia.Core/obj/Debug/net6.0/XAML/references");

            var typeSystem = new SreTypeSystem();

            var xamlLanguage = new XamlLanguageTypeMappings(typeSystem)
            {
                XmlnsAttributes =
                {
                    typeSystem.GetType(typeof(XmlnsDefinitionAttribute)),
                },
                ContentAttributes =
                {
                    typeSystem.GetType(typeof(ContentAttribute))
                },
                UsableDuringInitializationAttributes =
                {
                    typeSystem.GetType(typeof(UsableDuringInitializationAttribute))
                },
                DeferredContentPropertyAttributes =
                {
                    typeSystem.GetType(typeof(DeferredContentAttribute))
                },
            };

            var emitConfig = new XamlLanguageEmitMappings<IXamlILEmitter, XamlILNodeEmitResult>
            {
                ContextTypeBuilderCallback = (b, c) => EmitNameScopeField(xamlLanguage, typeSystem, b, c)
            };

            var transformerConfig = new TransformerConfiguration(
                typeSystem,
                typeSystem.GetAssembly(asm),
                xamlLanguage,
                XamlXmlnsMappings.Resolve(typeSystem, xamlLanguage), Parsing.CustomValueConverter);

            var compiler =
                new OpenNefiaXamlILCompiler(transformerConfig, emitConfig, true);

            var xamlPath = "C:/Users/yuno/build/OpenNefia.NET/OpenNefia.Core/ControlTest/TextureRectWindow.xaml";
            var res = new Resource(asm, typeof(TextureRectWindow), xamlPath);

            var xaml = new StreamReader(new MemoryStream(res.FileContents)).ReadToEnd();
            var parsed = XDocumentXamlParser.Parse(xaml);

            var classType = GetClassTypeFromXaml(res, parsed, typeSystem);

            compiler.Transform(parsed);

            var module = AssemblyBuilder
                .DefineDynamicAssembly(
                    new AssemblyName(Guid.NewGuid().ToString()),
                    AssemblyBuilderAccess.RunAndCollect)
                .DefineDynamicModule(Guid.NewGuid().ToString());

            var typeBuilder = module
                .DefineType(
                    Guid.NewGuid().ToString(),
                    TypeAttributes.Class | TypeAttributes.Public);

            var xamlTypeBuilder = typeSystem.CreateTypeBuilder(typeBuilder);

            var contextClass = XamlILContextDefinition.GenerateContextClass(xamlTypeBuilder, typeSystem,
                xamlLanguage, emitConfig);

            var type2 = module
                .DefineType(
                    Guid.NewGuid().ToString(),
                    TypeAttributes.Class | TypeAttributes.Public);
            var xamlType2Builder = typeSystem.CreateTypeBuilder(type2);

            var populateName = $"Populate:{res.Name}";
            var populateMethod = compiler.DefinePopulateMethod(xamlType2Builder, parsed, populateName, isPublic: false);

            compiler.Compile(parsed, contextClass,
                populateMethod,
                buildMethod: null,
                namespaceInfoBuilder: null,
                createClosure: null,
                createDelegateType: (closureName, closureBaseType, emitter) => xamlTypeBuilder.DefineSubType(closureBaseType, closureName, false),
                res.Uri, res
            );

            var compiledPopulateMethod = type2.CreateTypeInfo()!.DeclaredMethods
                .Where(m => m.Name == populateName).First();

            Patch.Context = compiledPopulateMethod;

            var harmony = new Harmony("xyz.opennefia");
            var original = typeof(TextureRectWindow).GetMethod(populateName, BindingFlags.NonPublic | BindingFlags.Static);
            var transpiler = typeof(Patch).GetMethod("Transpiler", BindingFlags.NonPublic | BindingFlags.Static);
            harmony.Patch(original, transpiler: new HarmonyMethod(transpiler));
            Logger.Info("Patched trampoline.");
        }

        private static IXamlType GetClassTypeFromXaml(IResource res, XamlDocument parsed, SreTypeSystem typeSystem)
        {
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

            var classType = typeSystem.FindType(classname);
            if (classType == null)
                throw new Exception($"Unable to find type '{classname}'");
            return classType;
        }
    }

    internal class Resource : IResource
    {
        private Assembly _assembly;
        private Type _classType;
        private string _xamlPath;

        public string Uri => $"resm:{Name}?assembly={_assembly.GetName().Name}";
        public string Name => $"{_classType.FullName}.xaml";
        public string FilePath => _xamlPath;
        public byte[] FileContents => File.ReadAllBytes(_xamlPath);

        public Resource(Assembly assembly, Type classType, string xamlPath)
        {
            this._assembly = assembly;
            this._classType = classType;
            this._xamlPath = xamlPath;
        }

        public void Remove()
        {
        }
    }

    internal class DummyBuildEngine : IBuildEngine
    {
        public bool ContinueOnError => false;
        public int LineNumberOfTaskNode => 0;
        public int ColumnNumberOfTaskNode => 0;
        public string ProjectFileOfTaskNode => string.Empty;

        public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
        {
            throw new NotImplementedException();
        }

        public void LogMessageEvent(BuildMessageEventArgs e)
        {
            Logger.InfoS("wisp.compile", e.Message);
        }

        public void LogWarningEvent(BuildWarningEventArgs e)
        {
            Logger.WarningS("wisp.compile", e.Message);
        }

        public void LogErrorEvent(BuildErrorEventArgs e)
        {
            Logger.ErrorS("wisp.compile", e.Message);
        }

        public void LogCustomEvent(CustomBuildEventArgs e)
        {
            Logger.InfoS("wisp.compile", e.Message);
        }
    }

    internal static class Patch
    {
        internal static MethodInfo Context { get; set; } = default!;

        private static Sre.Label MakeLabel(int i)
        {
            var constructor = typeof(Sre.Label).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, new Type[] { typeof(int) }, null)!;

            return (Sre.Label)constructor.Invoke(new object[] { i });
        }

        [UsedImplicitly]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var monoInstructions = Context.GetInstructions();
            var harmonyInstructions = monoInstructions.Select(ToHarmonyInstruction).ToList();

            var lookup = new Dictionary<int, int>();
            var i = 0;
            foreach (var inst in monoInstructions)
            {
                lookup[inst.Offset] = i;
            }

            var labelLookup = new Dictionary<int, Sre.Label>();
            var labelIndex = 0;
            foreach (var instruction in harmonyInstructions)
            {
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

            return harmonyInstructions;
        }

        private static CodeInstruction ToHarmonyInstruction(Mono.Reflection.Instruction monoInst)
        {
            return new CodeInstruction(monoInst.OpCode, monoInst.Operand);
        }
    }
}
