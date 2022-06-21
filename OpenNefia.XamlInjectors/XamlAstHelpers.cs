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

                // "My.GenericClass{T1, T2, T3}"
                var genericBracePos = classname.IndexOf("{");
                if (genericBracePos > 0)
                {
                    var genericStr = classname.Substring(genericBracePos);
                    classname = classname.Substring(0, genericBracePos);

                    // In the CLR, generic types are identified like "List`1" for 1 generic parameter,
                    // "Dictionary`2" for 2 parameters, and so on.
                    // https://stackoverflow.com/a/1483451
                    var generics = genericStr.Trim('{', '}').Split(',').Select(s => s.Trim()).ToList();
                    var genericSuffix = $"`{generics.Count}";
                    classname += genericSuffix;
                }
            }
            else
            {
                classname = xamlName.Replace(".xaml", "");
            }

            return classname;
        }
    }
}
