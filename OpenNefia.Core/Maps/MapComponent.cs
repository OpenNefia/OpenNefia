using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;

namespace OpenNefia.Core.Maps
{
    /// <summary>
    ///     Represents a map inside the ECS system.
    /// </summary>
    [ComponentUsage(ComponentTarget.Map)]
    public class MapComponent : Component
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

    public class MapMetadata
    {
        public readonly string Name;
        public readonly string Author;

        /// <summary>
        /// ModId -> LatestMigrationFilename
        /// </summary>
        public readonly Dictionary<string, string> Migrations;

        public MapMetadata(string name = "", string author = "", Dictionary<string, string>? migrations = null)
        {
            Name = name;
            Author = author;
            Migrations = migrations ?? new();
        }
    }
}
