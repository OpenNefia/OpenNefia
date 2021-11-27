namespace OpenNefia.Core.Rendering
{
    public interface IRegionSpecifier
    {
        public AssetRegions GetRegions(int width, int height);
    }
}