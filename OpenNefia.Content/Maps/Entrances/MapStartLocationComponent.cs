using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Maps
{
    [RegisterComponent]
    public class MapStartLocationComponent : Component
    {
        public override string Name => "MapStartLocation";

        [DataField(required: true)]
        public IMapStartLocation StartLocation { get; set; } = new CenterMapLocation();
    }
}