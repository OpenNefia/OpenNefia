namespace OpenNefia.Core.Rendering.TileRowDrawLayers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class RegisterTileRowLayerAttribute : Attribute
    {
        public readonly TileRowLayerType Type;
        public readonly Type[] RenderBefore;
        public readonly Type[] RenderAfter;
        public readonly bool EnabledAtStartup;

        public RegisterTileRowLayerAttribute(TileRowLayerType type, Type[]? renderBefore = null, Type[]? renderAfter = null, bool enabledAtStartup = true)
        {
            Type = type;
            RenderBefore = renderBefore ?? new Type[0];
            RenderAfter = renderAfter ?? new Type[0];
            EnabledAtStartup = enabledAtStartup;
        }
    }

    public enum TileRowLayerType
    {
        Tile,
        Chip
    }
}
