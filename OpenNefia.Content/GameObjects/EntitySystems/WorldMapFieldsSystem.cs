using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;

using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maps;
using OpenNefia.Content.FieldMap;
using OpenNefia.Core.Log;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Maps;

namespace OpenNefia.Content.GameObjects
{
    public interface IWorldMapFieldsSystem
    {
        PrototypeId<FieldTypePrototype> GetFieldMapFromStoodTile(PrototypeId<TilePrototype> stoodTile);
    }

    public class WorldMapFieldsSystem : EntitySystem, IWorldMapFieldsSystem
    {
        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly MapEntranceSystem _mapEntrances = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public override void Initialize()
        {
            SubscribeComponent<WorldMapFieldsComponent, GetVerbsEventArgs>(HandleGetVerbs);
        }

        private void HandleGetVerbs(EntityUid uid, WorldMapFieldsComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(StairsSystem.VerbTypeActivate, "Enter Field", () => EnterField(args.Source, args.Target)));
        }

        public PrototypeId<FieldTypePrototype> GetFieldMapFromStoodTile(PrototypeId<TilePrototype> stoodTile)
        {
            foreach (var proto in _protos.EnumeratePrototypes<FieldTypePrototype>())
            {
                if (proto.WorldMapTiles.Contains(stoodTile))
                    return proto.GetStrongID();
            }

            return Protos.FieldMap.Plains;
        }

        private TurnResult EnterField(EntityUid user, EntityUid mapEntity,
            WorldMapFieldsComponent? worldMapFields = null,
            SpatialComponent? userSpatial = null)
        {
            if (!Resolve(mapEntity, ref worldMapFields))
                return TurnResult.Failed;

            if (!Resolve(user, ref userSpatial))
                return TurnResult.Failed;

            if (!_mapManager.TryGetMap(userSpatial.MapID, out var map))
                return TurnResult.Failed;

            var prevCoords = userSpatial.MapPosition;

            var stoodTile = map.Tiles[prevCoords.X, prevCoords.Y].ResolvePrototype().GetStrongID();

            // TODO hack
            var gen = new FieldMapGenerator()
            {
                FieldMap = GetFieldMapFromStoodTile(stoodTile)
            };
            EntitySystem.InjectDependencies(gen);
            var fieldMap = gen.GenerateAndPopulate(new MapGeneratorOptions()
            {
                Width = 34,
                Height = 22
            });

            if (fieldMap == null)
            {
                Logger.WarningS("sys.field", "Map generation failed");
                return TurnResult.Failed;
            }

            var ev = new AfterFieldMapGeneratedEvent(gen.FieldMap);
            RaiseEvent(fieldMap.MapEntityUid, ev);

            var entrance = new MapEntrance()
            {
                MapIdSpecifier = new BasicMapIdSpecifier(fieldMap.Id),
                StartLocation = new CenterMapLocation()
            };

            if (_mapEntrances.UseMapEntrance(user, entrance, out var mapId))
            {
                _mapEntrances.SetPreviousMap(mapId.Value, prevCoords);
                return TurnResult.Succeeded;
            }

            return TurnResult.Aborted;
        }
    }

    [EventUsage(EventTarget.Map)]
    public sealed class AfterFieldMapGeneratedEvent : EntityEventArgs
    {
        public PrototypeId<FieldTypePrototype> FieldMap { get; }

        public AfterFieldMapGeneratedEvent(PrototypeId<FieldTypePrototype> fieldMap)
        {
            FieldMap = fieldMap;
        }
    }
}
