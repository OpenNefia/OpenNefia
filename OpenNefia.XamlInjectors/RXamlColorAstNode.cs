using System.Reflection.Emit;
using XamlX.Ast;
using XamlX.Emit;
using XamlX.IL;
using XamlX.TypeSystem;

namespace OpenNefia.Build.Tasks
{
    internal class RXamlColorAstNode
        : XamlAstNode, IXamlAstValueNode, IXamlAstILEmitableNode
    {
        private readonly IXamlMethod _method;
        private readonly string _color;

        public RXamlColorAstNode(IXamlLineInfo lineInfo, ONXamlWellKnownTypes types, string color) : base(lineInfo)
        {
            _color = color;
            Type = new XamlAstClrTypeReference(lineInfo, types.Color, false);
            _method = types.ColorFromXaml;
        }

        public IXamlAstTypeReference Type { get; }

        public XamlILNodeEmitResult Emit(XamlEmitContext<IXamlILEmitter, XamlILNodeEmitResult> context, IXamlILEmitter codeGen)
        {
            codeGen.Ldstr(_color);
            codeGen.EmitCall(_method);

            return XamlILNodeEmitResult.Type(0, Type.GetClrType());
        }
    }
}
