using System.Linq;
using XamlX.Ast;
using XamlX.Emit;
using XamlX.IL;
using XamlX.Transform;
using XamlX.TypeSystem;

namespace OpenNefia.XamlInjectors.CompilerExtensions.Transformers
{
    class AddNameScopeRegistration : IXamlAstTransformer
    {
        public IXamlAstNode Transform(AstTransformationContext context, IXamlAstNode node)
        {
            if (node is XamlPropertyAssignmentNode pa)
            {
                if (pa.Property.Name == "Name"
                    && pa.Property.DeclaringType.FullName == Constants.ControlTypeName)
                {
                    if (context.ParentNodes().FirstOrDefault() is XamlManipulationGroupNode mg
                        && mg.Children.OfType<OpenNefiaNameScopeRegistrationXamlIlNode>().Any())
                        return node;

                    IXamlAstValueNode value = null;
                    for (var c = 0; c < pa.Values.Count; c++)
                        if (pa.Values[c].Type.GetClrType().Equals(context.Configuration.WellKnownTypes.String))
                        {
                            value = pa.Values[c];
                            if (!(value is XamlAstTextNode))
                            {
                                var local = new XamlAstCompilerLocalNode(value);
                                // Wrap original in local initialization
                                pa.Values[c] = new XamlAstLocalInitializationNodeEmitter(value, value, local);
                                // Use local
                                value = local;
                            }

                            break;
                        }

                    if (value != null)
                    {
                        var objectType = context.ParentNodes().OfType<XamlAstConstructableObjectNode>().FirstOrDefault()?.Type.GetClrType();
                        return new XamlManipulationGroupNode(pa)
                        {
                            Children =
                                {
                                    pa,
                                    new OpenNefiaNameScopeRegistrationXamlIlNode(value, objectType)
                                }
                        };
                    }
                }
                /*else if (pa.Property.CustomAttributes.Select(attr => attr.Type).Intersect(context.Configuration.TypeMappings.DeferredContentPropertyAttributes).Any())
                {
                    pa.Values[pa.Values.Count - 1] =
                        new NestedScopeMetadataNode(pa.Values[pa.Values.Count - 1]);
                }*/
            }

            return node;
        }

        class OpenNefiaNameScopeRegistrationXamlIlNode : XamlAstNode, IXamlAstManipulationNode
        {
            public IXamlAstValueNode Name { get; set; }
            public IXamlType TargetType { get; }

            public OpenNefiaNameScopeRegistrationXamlIlNode(IXamlAstValueNode name, IXamlType targetType) : base(name)
            {
                TargetType = targetType;
                Name = name;
            }

            public override void VisitChildren(IXamlAstVisitor visitor)
                => Name = (IXamlAstValueNode)Name.Visit(visitor);
        }

        internal class Emitter : IXamlAstLocalsNodeEmitter<IXamlILEmitter, XamlILNodeEmitResult>
        {
            public XamlILNodeEmitResult Emit(IXamlAstNode node, XamlEmitContextWithLocals<IXamlILEmitter, XamlILNodeEmitResult> context, IXamlILEmitter codeGen)
            {
                if (node is OpenNefiaNameScopeRegistrationXamlIlNode registration)
                {

                    var scopeField = context.RuntimeContext.ContextType.Fields.First(f =>
                        f.Name == XamlCompiler.ContextNameScopeFieldName);
                    var namescopeRegisterFunction = context.Configuration.TypeSystem
                        .FindType(Constants.NameScopeTypeName).Methods
                        .First(m => m.Name == "Register");

                    using (var targetLoc = context.GetLocalOfType(context.Configuration.TypeSystem.FindType(Constants.ControlTypeName)))
                    {

                        codeGen
                            // var target = {pop}
                            .Stloc(targetLoc.Local)
                            // _context.NameScope.Register(Name, target)
                            .Ldloc(context.ContextLocal)
                            .Ldfld(scopeField);

                        context.Emit(registration.Name, codeGen, registration.Name.Type.GetClrType());

                        codeGen
                            .Ldloc(targetLoc.Local)
                            .EmitCall(namescopeRegisterFunction, true);
                    }

                    return XamlILNodeEmitResult.Void(1);
                }
                return default;
            }
        }
    }
}
