using Avalonia.Metadata;
using HarmonyLib;
using JetBrains.Annotations;
using Microsoft.Build.Framework;
using OpenNefia.Core.Log;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.UserInterface.XAML;
using OpenNefia.XamlInjectors;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;
using XamlX.IL;
using XamlX.Transform;
using static OpenNefia.XamlInjectors.XamlCompiler;
using Cil = Mono.Cecil.Cil;
using Sre = System.Reflection.Emit;

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

        private bool _patched = false;

        private void DoPatch(BaseButton.ButtonEventArgs obj)
        {
            if (_patched)
                return;

            var asm = typeof(Engine).Assembly;
            File.Copy(asm.Location, asm.Location + ".copy", overwrite: true);

            var references = File.ReadAllLines("C:/Users/yuno/build/OpenNefia.NET/OpenNefia.Core/obj/Debug/net6.0/XAML/references");

            var xamlCompiler = new XamlCompiler(asm.Location + ".copy", references, new DummyBuildEngine());

            var xamlPath = "C:/Users/yuno/build/OpenNefia.NET/OpenNefia.Core/ControlTest/TextureRectWindow.xaml";
            var res = new Resource(asm, typeof(TextureRectWindow), xamlPath);

            var typeSystem = xamlCompiler.TypeSystem;

            var contextDef = typeSystem.GetTypeReference(typeSystem.FindType(Constants.CompiledXamlNamespace + ".XamlIlContext")).Resolve();

            var contextClass = XamlILContextDefinition.GenerateContextClass(typeSystem.CreateTypeBuilder(contextDef), typeSystem,
                xamlCompiler.XamlLanguage, xamlCompiler.EmitConfig);

            var result = xamlCompiler.CompileSingleResource(res, contextClass);

            Patch.Context = result;

            var harmony = new Harmony("xyz.opennefia");
            var original = typeof(TextureRectWindow).GetMethod(result.CompiledPopulateMethod.Name, BindingFlags.NonPublic | BindingFlags.Static);
            var transpiler = typeof(Patch).GetMethod("Transpiler", BindingFlags.NonPublic | BindingFlags.Static);
            harmony.Patch(original, transpiler: new HarmonyMethod(transpiler));
            Logger.Info("Patched trampoline.");

            _patched = true;
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

    internal class SreOpCodeConverter
    {
        private readonly Dictionary<Cil.Code, Sre.OpCode> _lookup = new();

        public SreOpCodeConverter()
        {
            var fields = typeof(Sre.OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public);
           
            foreach (var field in fields.Where(f => f.FieldType == typeof(Sre.OpCode)))
            {
                var cecilCode = GetCecilCode(field.Name);
                var sreOpCode = (Sre.OpCode)field.GetValue(null)!;

                if (cecilCode != null)
                {
                    _lookup.Add(cecilCode.Value, sreOpCode);
                }
                else
                {
                    Logger.WarningS("wisp.compile", $"Missing opcode with name {field.Name}");
                }
            }
        }

        public Sre.OpCode this[Cil.OpCode cecilOpCode] => _lookup[cecilOpCode.Code];

        private static Cil.Code? GetCecilCode(string name)
        {
            Cil.Code code;
            return Enum.TryParse(name, true, out code) ? code : null;
        }
    }

    internal static class Patch
    {
        internal static CompileGroupResult Context { get; set; } = default!;
        internal static SreOpCodeConverter SreOpCodes = new();

        [UsedImplicitly]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return Context.CompiledPopulateMethod.Body.Instructions.Select(ToHarmonyInstruction);
        }

        private static CodeInstruction ToHarmonyInstruction(Cil.Instruction cecilInst)
        {
            var operand = cecilInst.Operand;

            if (operand is Mono.Cecil.MethodReference method)
{
                var type = Type.GetType(method.DeclaringType.FullName);
                var parameters = method.Parameters.Select(param => param.ParameterType.GetMonoType()).ToArray();
                var genericParameters = method.GenericParameters.Select(param => param.GetMonoType()).ToArray();
                operand = AccessTools.Method(type, method.Name, parameters, genericParameters);
            }

            return new CodeInstruction(SreOpCodes[cecilInst.OpCode], operand);
        }

        public static Type GetMonoType(this Mono.Cecil.TypeReference type)
        {
            return Type.GetType(type.GetReflectionName(), true)!;
        }

        private static string GetReflectionName(this Mono.Cecil.TypeReference type)
        {
            if (type.IsGenericInstance)
            {
                var genericInstance = (Mono.Cecil.GenericInstanceType)type;
                return string.Format("{0}.{1}[{2}]", genericInstance.Namespace, type.Name, string.Join(",", genericInstance.GenericArguments.Select(p => p.GetReflectionName()).ToArray()));
            }
            return type.FullName;
        }
    }
}
