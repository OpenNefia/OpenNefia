using JetBrains.Annotations;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Core.UserInterface
{
    /// <summary>
    /// XAML extension for retrieving assets in XAML.
    /// This lets you do "{Asset 'Elona.AutoTurnIcon'}" inline.
    /// </summary>
    [PublicAPI]
    public sealed class AssetExtension
    {
        public string Key { get; }

        public AssetExtension(string key)
        {
            Key = key;
        }

        public object ProvideValue()
        {
            return Assets.Get(new(Key));
        }
    }
}
