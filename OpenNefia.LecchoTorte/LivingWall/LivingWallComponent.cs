using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.LecchoTorte.LivingWall
{
    [RegisterComponent]
    public class LivingWallComponent : Component
    {
        public override string Name => "LecchoTorte.LivingWall";

        [DataField(required: true)]
        public PrototypeId<TilePrototype> TileID { get; set; } = Protos.Tile.WallDirt;

        [DataField]
        public PrototypeId<TilePrototype>? TileStandingOn { get; set; }
    }
}
