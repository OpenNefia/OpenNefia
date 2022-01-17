namespace OpenNefia.Core.Rendering
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class RegisterTileLayerAttribute : Attribute
    {
        public readonly Type[] RenderBefore;
        public readonly Type[] RenderAfter;
        public readonly bool EnabledAtStartup;

        public RegisterTileLayerAttribute(Type[]? renderBefore = null, Type[]? renderAfter = null, bool enabledAtStartup = true)
        {
            RenderBefore = renderBefore ?? new Type[0];
            RenderAfter = renderAfter ?? new Type[0];
            EnabledAtStartup = enabledAtStartup;
        }
    }
}
