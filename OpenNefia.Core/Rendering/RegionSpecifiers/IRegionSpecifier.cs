using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Rendering
{
    [ImplicitDataDefinitionForInheritors]
    public interface IRegionSpecifier
    {
        public AssetRegions GetRegions(Vector2i size);
    }
}