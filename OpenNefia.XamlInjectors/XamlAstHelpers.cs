using System.Linq;
using XamlX.Ast;
using static OpenNefia.XamlInjectors.XamlCompiler;

namespace OpenNefia.XamlInjectors
{
    public static class XamlAstHelpers
    {
        public static string GetClassNameFromXaml(string xamlName, XamlDocument parsed)
{
            XamlAstObjectNode initialRoot = (XamlAstObjectNode)parsed.Root;

            // Look for an unnamespaced "Class='<...>'" directive (we do not use the Xaml2006 namespace).
            var property = initialRoot.Children.OfType<XamlAstXamlPropertyValueNode>()
                .FirstOrDefault(p => p.Property is XamlAstNamePropertyReference namedProperty && namedProperty.Name == "Class");
            string classname;
            if (property != null && property.Values[0] is XamlAstTextNode tn)
            {
                classname = tn.Text;
            }
            else
            {
                classname = xamlName.Replace(".xaml", "");
            }

            return classname;
        }
    }
}
