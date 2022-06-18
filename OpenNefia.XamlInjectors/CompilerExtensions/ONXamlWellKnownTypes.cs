using System.Linq;
using XamlX.Transform;
using XamlX.TypeSystem;

namespace OpenNefia.XamlInjectors.CompilerExtensions
{
    public class ONXamlWellKnownTypes
    {
        public XamlTypeWellKnownTypes XamlIlTypes { get; }
        public IXamlType Single { get; }
        public IXamlType Int32 { get; }
        public IXamlType Vector2 { get; }
        public IXamlConstructor Vector2ConstructorFull { get; }
        public IXamlType Vector2i { get; }
        public IXamlConstructor Vector2iConstructorFull { get; }
        public IXamlType Thickness { get; }
        public IXamlConstructor ThicknessConstructorFull { get; }
        public IXamlType Color { get; }
        public IXamlMethod ColorFromXaml { get; }

        public ONXamlWellKnownTypes(TransformerConfiguration cfg)
        {
            var ts = cfg.TypeSystem;
            XamlIlTypes = cfg.WellKnownTypes;
            Single = ts.GetType("System.Single");
            Int32 = ts.GetType("System.Int32");

            (Vector2, Vector2ConstructorFull) = GetNumericTypeInfo("OpenNefia.Core.Maths.Vector2", Single, 2);
            (Vector2i, Vector2iConstructorFull) = GetNumericTypeInfo("OpenNefia.Core.Maths.Vector2i", Int32, 2);
            (Thickness, ThicknessConstructorFull) = GetNumericTypeInfo("OpenNefia.Core.Maths.Thickness", Single, 4);

            (IXamlType, IXamlConstructor) GetNumericTypeInfo(string name, IXamlType componentType, int componentCount)
            {
                var type = cfg.TypeSystem.GetType(name);
                var ctor = type.GetConstructor(Enumerable.Repeat(componentType, componentCount).ToList());

                return (type, ctor);
            }

            Color = cfg.TypeSystem.GetType("OpenNefia.Core.Maths.Color");
            ColorFromXaml = Color.GetMethod(new FindMethodMethodSignature("FromXaml", Color, XamlIlTypes.String)
            {
                IsStatic = true
            });
        }
    }

    public static class ONXamlWellKnownTypesExtensions
    {
        public static ONXamlWellKnownTypes GetOpenNefiaTypes(this AstTransformationContext ctx)
        {
            if (ctx.TryGetItem<ONXamlWellKnownTypes>(out var rv))
                return rv;
            ctx.SetItem(rv = new ONXamlWellKnownTypes(ctx.Configuration));
            return rv;
        }
    }
}
