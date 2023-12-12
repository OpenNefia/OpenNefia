using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;

namespace OpenNefia.Core.Maps
{
    /// <summary>
    ///     Represents a map inside the ECS system.
    /// </summary>
    [ComponentUsage(ComponentTarget.Map)]
    public sealed class MapComponent : Component
    {
        [DataField("mapId")]
        private MapId _mapId = MapId.Nullspace;

        public MapId MapId
        {
            get => _mapId;
            internal set => _mapId = value;
        }

        public MapMetadata Metadata { get; internal set; } = new();
    }

    [DataDefinition]
    public sealed class MapMetadata
    {
        [DataField("format")]
        public int Format { get; }

        [DataField("name")]
        public string Name { get; } = string.Empty;

        [DataField("author")]
        public string Author { get; } = string.Empty;

        public MapMetadata()
        {
        }

        public MapMetadata(string name, string author)
        {
            Name = name;
            Author = author;
        }
    }
}
