﻿using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;

namespace OpenNefia.Core.GameObjects
{   
    /// <summary>
    ///     Represents a map inside the ECS system.
    /// </summary>
    public class MapComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "Map";

        [DataField("mapId")]
        private MapId _mapId = MapId.Nullspace;

        public MapId MapId
        {
            get => _mapId;
            internal set => _mapId = value;
        }

        public MapMetadata Metadata { get; internal set; } = new();

        public IMapStartLocation StartLocation { get; set; } = new CenterMapLocation();

        /// <inheritdoc />
        public void ClearMapId()
        {
            _mapId = MapId.Nullspace;
        }
    }
    
    public class MapMetadata
    {
        public readonly string Name;
        public readonly string Author;

        public MapMetadata(string name = "", string author = "")
        {
            Name = name;
            Author = author;
        }
    }
}
