using OpenNefia.Build.Tasks;
using OpenNefia.XamlInjectors.CompilerExtensions;
using System.Linq;
using XamlX.Ast;
using XamlX.Emit;
using XamlX.IL;
using XamlX.Transform;

namespace OpenNefia.XamlInjectors.Compiler
{
    class OpenNefiaMarkRootObjectScopeNode : IXamlAstTransformer
    {
        public IXamlAstNode Transform(AstTransformationContext context, IXamlAstNode node)
        {
            if (!context.ParentNodes().Any()
                && node is XamlValueWithManipulationNode mnode)
            {
                mnode.Manipulation = new XamlManipulationGroupNode(mnode,
                    new[]
                    {
                            mnode.Manipulation,
                            new HandleRootObjectScopeNode(mnode)
                    });
            }
            return node;
        }
        class HandleRootObjectScopeNode : XamlAstNode, IXamlAstManipulationNode
        {
            public HandleRootObjectScopeNode(IXamlLineInfo lineInfo) : base(lineInfo)
            {
            }
        }
        internal class Emitter : IXamlILAstNodeEmitter
        {
            public XamlILNodeEmitResult Emit(IXamlAstNode node, XamlEmitContext<IXamlILEmitter, XamlILNodeEmitResult> context, IXamlILEmitter codeGen)
            {
                if (!(node is HandleRootObjectScopeNode))
                {
                    return null;
                }

                var controlType = context.Configuration.TypeSystem.FindType("OpenNefia.Core.UI.Element.UiElement");

                var next = codeGen.DefineLabel();
                var dontAbsorb = codeGen.DefineLabel();
                var end = codeGen.DefineLabel();
                var contextScopeField = context.RuntimeContext.ContextType.Fields.First(f =>
                    f.Name == OpenNefiaXamlIlLanguage.ContextNameScopeFieldName);
                var controlNameScopeField = controlType.Fields.First(f => f.Name == "NameScope");
                var nameScopeType = context.Configuration.TypeSystem
                    .FindType("OpenNefia.Core.UserInterface.XAML.NameScope");
                var nameScopeCompleteMethod = nameScopeType.Methods.First(m => m.Name == "Complete");
                var nameScopeAbsorbMethod = nameScopeType.Methods.First(m => m.Name == "Absorb");
                using (var local = codeGen.LocalsPool.GetLocal(controlType))
                {
                    codeGen
                        .Isinst(controlType)
                        .Dup()
                        .Stloc(local.Local) //store control in local field
                        .Brfalse(next) //if control is null, move to next (this should never happen but whatev, avalonia does it)
                        .Ldloc(context.ContextLocal)
                        .Ldfld(contextScopeField)
                        .Ldloc(local.Local) //load control from local field
                        .Ldfld(controlNameScopeField) //load namescope field from control
                        .EmitCall(nameScopeAbsorbMethod, true)
                        .Ldloc(local.Local) //load control
                        .Ldloc(context.ContextLocal) //load contextObject
                        .Ldfld(contextScopeField) //load namescope field from context obj
                        .Stfld(controlNameScopeField) //store namescope field in control
                        .MarkLabel(next)
                        .Ldloc(context.ContextLocal)
                        .Ldfld(contextScopeField)
                        .EmitCall(nameScopeCompleteMethod, true); //set the namescope as complete
                }

                return XamlILNodeEmitResult.Void(1);
            }
        }
    }
}
