namespace OpenNefia.Core.Rendering
{
    public record AtlasRegion(string atlasName, string id, TileSpecifier spec, bool hasOverhang = false);

    internal interface IAtlasRegionProvider
    {
        IEnumerable<AtlasRegion> GetAtlasRegions();
    }
}