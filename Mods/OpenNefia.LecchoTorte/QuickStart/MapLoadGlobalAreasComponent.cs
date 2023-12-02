using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.LecchoTorte.QuickStart
{
    /// <summary>
    /// Loads the specified list of global areas when this map is first entered.
    /// </summary>
    [RegisterComponent]
    [ComponentProtoName("LecchoTorte.MapLoadGlobalAreas")]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class MapLoadGlobalAreasComponent : Component
    {
        [DataField]
        public List<GlobalAreaId> InitGlobalAreas { get; } = new();
    }
}