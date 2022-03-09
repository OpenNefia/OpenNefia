using OpenNefia.XamlInjectors.CompilerExtensions;
using Pidgin;
using System.Diagnostics.CodeAnalysis;
using XamlX;
using XamlX.Ast;
using XamlX.Transform;
using XamlX.TypeSystem;

namespace OpenNefia.Core.UserInterface.XAML.HotReload
{
    internal static class Parsing
    {
        internal static bool TryConvert(AstTransformationContext context, IXamlAstValueNode node, string text, IXamlType type, ONXamlWellKnownTypes types, [NotNullWhen(true)] out IXamlAstValueNode? result)
        {
            if (type.Equals(types.Vector2))
            {
                var foo = MathParsing.Single2.Parse(text);

                if (!foo.Success)
                    throw new XamlLoadException($"Unable to parse \"{text}\" as a Vector2", node);

                var (x, y) = foo.Value;

                result = new ONXamlSingleVecLikeConstAstNode(
                    node,
                    types.Vector2, types.Vector2ConstructorFull,
                    types.Single, new[] { x, y });
                return true;
            }

            if (type.Equals(types.Thickness))
            {
                var foo = MathParsing.Thickness.Parse(text);

                if (!foo.Success)
                    throw new XamlLoadException($"Unable to parse \"{text}\" as a Thickness", node);

                var val = foo.Value;
                float[] full;
                if (val.Length == 1)
                {
                    var u = val[0];
                    full = new[] { u, u, u, u };
                }
                else if (val.Length == 2)
                {
                    var h = val[0];
                    var v = val[1];
                    full = new[] { h, v, h, v };
                }
                else // 4
                {
                    full = val;
                }

                result = new ONXamlSingleVecLikeConstAstNode(
                    node,
                    types.Thickness, types.ThicknessConstructorFull,
                    types.Single, full);
                return true;
            }

            if (type.Equals(types.Thickness))
            {
                var foo = MathParsing.Thickness.Parse(text);

                if (!foo.Success)
                    throw new XamlLoadException($"Unable to parse \"{text}\" as a Thickness", node);

                var val = foo.Value;
                float[] full;
                if (val.Length == 1)
                {
                    var u = val[0];
                    full = new[] { u, u, u, u };
                }
                else if (val.Length == 2)
                {
                    var h = val[0];
                    var v = val[1];
                    full = new[] { h, v, h, v };
                }
                else // 4
                {
                    full = val;
                }

                result = new ONXamlSingleVecLikeConstAstNode(
                    node,
                    types.Thickness, types.ThicknessConstructorFull,
                    types.Single, full);
                return true;
            }

            if (type.Equals(types.Color))
            {
                // TODO: Interpret these colors at XAML compile time instead of at runtime.
                result = new ONXamlColorAstNode(node, types, text);
                return true;
            }

            result = null;
            return false;
        }
    }
}