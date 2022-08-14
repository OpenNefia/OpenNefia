using OpenNefia.Content.Activity;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Rendering;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.Mining
{
    public interface IActionDigSystem : IEntitySystem
    {
        bool CheckMiningSuccess(EntityUid digger, MapCoordinates digCoords, int turnsSpentMining);
        EntityUid? CreateItemFromMinedWall(MapCoordinates digCoords);
        void FinishMiningWall(EntityUid digger, MapCoordinates digCoords);
        TurnResult StartMining(EntityUid player, MapCoordinates targetCoords);
    }

    public sealed class ActionDigSystem : EntitySystem, IActionDigSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IMapTilesetSystem _tilesets = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IRandomGenSystem _randomGenSystem = default!;
        [Dependency] private readonly IMapDebrisSystem _mapDebris = default!;
        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;

        public override void Initialize()
        {
            SubscribeEntity<EntityFinishedMiningWallEvent>(CreateItemFromMinedWall);
        }

        public bool CheckMiningSuccess(EntityUid digger, MapCoordinates digPos, int turnsSpentMining)
        {
            if (!TryMap(digPos, out var map))
                return false;

            if (CompOrNull<MapMiningPreventionComponent>(map.MapEntityUid)?.Kind == MiningPreventionKind.PreventMining)
                return false;

            var tile = map.GetTilePrototype(digPos.Position);
            if (tile == null)
                return false;

            var success = false;

            // TODO configure difficulty
            var difficulty = 1500;
            var minTurnsMining = 20;
            if (tile.Kind == TileKind.HardWall)
            {
                difficulty = 12000;
                minTurnsMining = 30;
            }

            if (_rand.Next(difficulty) < _skills.Level(digger, Protos.Skill.AttrStrength) + _skills.Level(digger, Protos.Skill.Mining) * 10)
                success = true;

            var minTurns = minTurnsMining - _skills.Level(digger, Protos.Skill.Mining) / 2;
            if (minTurns > 0 && turnsSpentMining <= minTurns)
                success = false;

            return success;
        }

        private enum MinedItemKind
        {
            Item,
            Gold
        }

        private void CreateItemFromMinedWall(EntityUid uid, EntityFinishedMiningWallEvent ev)
        {
            CreateItemFromMinedWall(ev.TargetCoords);
        }

        public EntityUid? CreateItemFromMinedWall(MapCoordinates digCoords)
        {
            if (!TryMap(digCoords, out var map))
                return null;

            if (CompOrNull<MapMiningPreventionComponent>(map.MapEntityUid)?.Kind >= MiningPreventionKind.NoMinedItems)
                return null;

            MinedItemKind? kind = null;

            if (_rand.OneIn(5))
                kind = MinedItemKind.Gold;
            if (_rand.OneIn(8))
                kind = MinedItemKind.Item;

            if (kind == MinedItemKind.Gold)
            {
                return _itemGen.GenerateItem(digCoords, Protos.Item.GoldPiece);
            }
            else if (kind == MinedItemKind.Item)
            {
                var filter = new ItemFilter()
                {
                    MinLevel = _randomGenSystem.CalcObjectLevel(map),
                    Quality = _randomGenSystem.CalcObjectQuality(Qualities.Quality.Good),
                    Tags = new[] { Protos.Tag.ItemCatOre }
                };
                return _itemGen.GenerateItem(digCoords, filter);
            }

            return null;
        }

        public void FinishMiningWall(EntityUid digger, MapCoordinates digCoords)
        {
            if (!TryMap(digCoords, out var map))
                return;

            // Set the dug tile type to the "tunnel" tile of the map's tileset.
            var mapCommon = EntityManager.EnsureComponent<MapCommonComponent>(map.MapEntityUid);
            var tile = _tilesets.GetTile(Protos.Tile.MapgenTunnel, mapCommon.Tileset)!;
            map.SetTile(digCoords.Position, tile.Value);

            _mapDebris.SpillFragments(digCoords, 2);
            _audio.Play(Protos.Sound.Crush1, digCoords);
            var animBreaking = new BreakingFragmentsMapDrawable();
            _mapDrawables.Enqueue(animBreaking, digCoords);

            foreach (var spatial in _lookup.GetLiveEntitiesAtCoords(digCoords))
            {
                var dugIntoEv = new EntityWasMinedIntoEvent(digger);
                RaiseEvent(spatial.Owner, dugIntoEv);
            }

            _mes.Display(Loc.GetString("Elona.Dig.Mining.Finish.Wall"));
            var dugEv = new EntityFinishedMiningWallEvent(digCoords);
            RaiseEvent(digger, dugEv);

            _skills.GainSkillExp(digger, Protos.Skill.Mining, 100);
        }

        public TurnResult StartMining(EntityUid player, MapCoordinates targetCoords)
        {
            if (!TryMap(targetCoords, out var map))
                return TurnResult.Aborted;

            if (targetCoords == Spatial(player).MapPosition)
            {
                _activities.StartActivity(player, Protos.Activity.DiggingSpot);
                return TurnResult.Succeeded;
            }

            // Don't allow digging into water.
            // TODO ...and other tiles?
            var tile = map.GetTilePrototype(targetCoords.Position);
            var canDig = tile != null && tile.IsSolid && tile.Kind != TileKind.Water;

            if (!canDig)
            {
                _mes.Display(Loc.GetString("Elona.Common.ItIsImpossible"));
                return TurnResult.Aborted;
            }

            if (!TryComp<SkillsComponent>(player, out var skills) || skills.Stamina < 0)
            {
                _mes.Display(Loc.GetString("Elona.Dig.TooExhausted"));
                return TurnResult.Aborted;
            }

            var activity = EntityManager.SpawnEntity(Protos.Activity.Mining, MapCoordinates.Global);
            Comp<ActivityMiningComponent>(activity).TargetTile = targetCoords;
            _activities.StartActivity(player, activity);

            return TurnResult.Succeeded;
        }
    }

    public sealed class EntityWasMinedIntoEvent : EntityEventArgs
    {
        public EntityUid Digger { get; }

        public EntityWasMinedIntoEvent(EntityUid digger)
        {
            Digger = digger;
        }
    }

    public sealed class EntityFinishedMiningWallEvent
    {
        public MapCoordinates TargetCoords { get; }

        public EntityFinishedMiningWallEvent(MapCoordinates targetCoords)
        {
            TargetCoords = targetCoords;
        }
    }
}