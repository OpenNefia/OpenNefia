using XamlX;
using XamlX.Ast;
using XamlX.Transform;

namespace OpenNefia.XamlInjectors.Compiler
{
    class IgnoredDirectivesTransformer : IXamlAstTransformer
    {
        public IXamlAstNode Transform(AstTransformationContext context, IXamlAstNode node)
        {
            if (node is XamlAstObjectNode astNode)
            {
                astNode.Children.RemoveAll(n =>
                    n is XamlAstXmlDirective dir &&
                    dir.Namespace == XamlNamespaces.Xaml2006 &&
                    (dir.Name == "Class" ||
                        dir.Name == "Precompile" ||
                        dir.Name == "FieldModifier" ||
                        dir.Name == "ClassModifier"));
            }

            return node;
        }
    }
}
