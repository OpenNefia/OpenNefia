using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Core.Prototypes.EntityPrototype;

namespace OpenNefia.Core.Maps
{
    /// <summary>
    /// NOTE: Does not account for serialized map extra things yet
    /// </summary>
    [DataDefinition]
    public sealed class MapBlueprint
    {
        [DataField("meta", required: true)]
        public MapMetadata Metadata { get; set; } = new();

        [DataField("tilemap", required: true)]
        public Dictionary<string, PrototypeId<TilePrototype>> Tilemap { get; set; } = new();

        [DataField("grid", required: true)]
        public string Grid { get; set; } = "";

        [DataField("entities", required: true)]
        public List<MapBlueprintEntity> Entities { get; set; } = new();
    }

    [DataDefinition]
    public sealed class MapBlueprintEntity
    {
        [DataField("uid", required: true)]
        public EntityUid Uid { get; set; } = EntityUid.Invalid;

        [DataField("protoId")]
        public PrototypeId<EntityPrototype>? ProtoId { get; set; }

        [DataField("components")]
        public ComponentRegistry Components { get; set; } = new();

        public bool HasComponent<T>(IPrototypeManager? protos = null) where T: class, IComponent
        {
            IoCManager.Resolve(ref protos);

            return Components.HasComponent<T>()
                || (ProtoId != null && protos.TryIndex(ProtoId.Value, out var proto) && proto.Components.HasComponent<T>());
        }
    }
}
