using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.GameObjects;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.Log;

namespace OpenNefia.Content.Maps
{
    public interface IMapTilesetSystem : IEntitySystem
    {
        PrototypeId<MapTilesetPrototype> GetTileset(IMap map);

        // TODO I think this always needs to return a tile.
        bool TryGetTile(PrototypeId<TilePrototype> tileId, PrototypeId<MapTilesetPrototype> tilesetId, [NotNullWhen(true)] out PrototypeId<TilePrototype>? tile, bool noFallback = false);
        bool TryGetTile(PrototypeId<TilePrototype> tileId, IMap map, [NotNullWhen(true)] out PrototypeId<TilePrototype>? tile, bool noFallback = false);

        void ApplyTileset(IMap map);
        void ApplyTileset(IMap map, PrototypeId<MapTilesetPrototype> protoId);
    }

    public class MapTilesetSystem : EntitySystem, IMapTilesetSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public PrototypeId<MapTilesetPrototype> GetTileset(IMap map)
        {
            if (EntityManager.TryGetComponent<MapCommonComponent>(map.MapEntityUid, out var common))
                return common.Tileset;

            return Protos.MapTileset.Default;
        }

        public bool TryGetTile(PrototypeId<TilePrototype> tileId, PrototypeId<MapTilesetPrototype> tilesetId, [NotNullWhen(true)] out PrototypeId<TilePrototype>? tile, bool noFallback = false)
        {
            var tileset = _protos.Index(tilesetId);

            if (tileset.Tiles.TryGetValue(tileId, out var tilePicker))
            {
                tile = tilePicker.GetTile();
                return true;
            }

            if (!noFallback)
            {
                return TryGetTile(tileId, Protos.MapTileset.Default, out tile, noFallback: true);
            }

            Logger.ErrorS("tileset", $"Failed to find tile {tileId} in tileset {tilesetId}");
            tile = null;
            return false;
        }

        public bool TryGetTile(PrototypeId<TilePrototype> tileId, IMap map, [NotNullWhen(true)] out PrototypeId<TilePrototype>? tile, bool noFallback = false)
        {
            var tileset = GetTileset(map);
            return TryGetTile(tileId, tileset, out tile, noFallback);
        }

        public void ApplyTileset(IMap map)
        {
            if (!TryComp<MapCommonComponent>(map.MapEntityUid, out var mapCommon))
                return;

            ApplyTileset(map, mapCommon.Tileset);
        }

        public void ApplyTileset(IMap map, PrototypeId<MapTilesetPrototype> protoId)
        {
            var tileset = _protos.Index(protoId);

            if (tileset.FogTile != null)
            {
                ConvertFog(map, tileset.FogTile);
            }

            ConvertTiles(map, tileset);

            ConvertDoors(map, tileset.DoorChips);
        }

        private void ConvertFog(IMap map, ITilePicker fogTile)
        {
            foreach (var tileRef in map.AllTiles)
            {
                var tile = fogTile.GetTile();
                map.SetTileMemory(tileRef.Position, tile);
            }

            var common = EntityManager.EnsureComponent<MapCommonComponent>(map.MapEntityUid);
            common.FogTile = fogTile.GetTile();
        }

        private void ConvertTiles(IMap map, MapTilesetPrototype tileset)
        {
            var defaultTileset = _protos.Index(Protos.MapTileset.Default);
            foreach (var tileRef in map.AllTiles)
            {
                var tileId = tileRef.Tile.GetStrongID();
                tileset.Tiles.TryGetValue(tileId, out var match);

                // quirk in the original map generation algorithm: there are two
                // mapgen tile IDs for the map floor, but one is used for hidden
                // paths and another is used for walls.This is so when digging
                // tunnels the map generator doesn't accidentally dig into a
                // tile holding a hidden path, but still lets it appear like the
                // default tile.
                if (match == null && tileId == Protos.Tile.MapgenFog)
                {
                    tileset.Tiles.TryGetValue(Protos.Tile.MapgenDefault, out match);
                    if (match == null)
                    {
                        defaultTileset.Tiles.TryGetValue(Protos.Tile.MapgenDefault, out match);
                    }
                }

                if (match == null)
                {
                    defaultTileset.Tiles.TryGetValue(tileId, out match);
                }

                if (match != null)
                {
                    var newTile = match.GetTile();
                    map.SetTile(tileRef.Position, newTile);
                }
            }
        }

        private void ConvertDoors(IMap map, DoorChips doorChips)
        {
            foreach (var ent in _lookup.EntityQueryInMap<DoorComponent>(map.Id))
            {
                if (doorChips.ChipOpen != null)
                {
                    ent.ChipOpen = doorChips.ChipOpen.Value;
                }
                if (doorChips.ChipClosed != null)
                {
                    ent.ChipClosed = doorChips.ChipClosed.Value;
                }
            }
        }
    }
}
