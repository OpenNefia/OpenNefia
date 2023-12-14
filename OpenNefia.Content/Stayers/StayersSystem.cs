using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.TitleScreen;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Containers;
using OpenNefia.Core.Maths;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Core.Log;
using OpenNefia.Core.Game;
using static OpenNefia.Content.Prototypes.Protos;
using static System.Runtime.InteropServices.JavaScript.JSType;
using OpenNefia.Content.DisplayName;
using static NetVips.Enums;

namespace OpenNefia.Content.Stayers
{
    /// <summary>
    /// Manages characters that stay behind in player-owned locations.
    /// The player is able to assign one or more allies to stay in their home location
    /// without following them. This system manages loading and unloading the staying entities
    /// in a persistent global location when maps are changed so that the components of the
    /// entities remain available for querying.
    /// </summary>
    public interface IStayersSystem : IEntitySystem
    {
        /// <summary>
        /// Enumerates all *unloaded* stayers (moved into global container, no longer in map).
        /// </summary>
        /// <param name="tag">Tag to query for.</param>
        /// <returns></returns>
        IEnumerable<StayingComponent> EnumerateAllStayers(string tag);

        /// <summary>
        /// Returns true if the entity is marked for staying, regardless of loaded status.
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="tag"></param>
        /// <param name="staying"></param>
        /// <returns></returns>
        bool IsStaying(EntityUid ent, string? tag = null, StayingComponent? staying = null);

        void RegisterStayer(EntityUid ent, IMap map, string tag, Vector2i? pos = null, StayingComponent? staying = null);
        void RegisterStayer(EntityUid ent, MapCoordinates mapCoords, string areaName, string tag, StayingComponent? staying = null);
        void UnregisterStayer(EntityUid ent, StayingComponent? staying = null);
    }

    public sealed class StayersSystem : EntitySystem, IStayersSystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IMapPlacement _mapPlacement = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;

        [RegisterSaveData("Elona.StayerSystem.StayersEntity")]
        private EntityUid _stayersEntity { get; set; } = EntityUid.Invalid;
        private Container _stayersContainer => EnsureComp<StayersComponent>(_stayersEntity).Container;

        public override void Initialize()
        {
            SubscribeBroadcast<GameInitiallyLoadedEventArgs>(EnsureStayersContainer);

            SubscribeBroadcast<BeforeMapLeaveEventArgs>(TransferFromMapToStayers);
            SubscribeBroadcast<AfterMapEnterEventArgs>(TransferFromStayersToMap);
        }

        public bool IsStaying(EntityUid ent, string? tag = null, StayingComponent? staying = null)
        {
            if (!Resolve(ent, ref staying, logMissing: false))
                return false;

            if (staying.StayingLocation == null)
                return false;

            if (tag != null && staying.StayingLocation.Tag != tag)
                return false;

            return true;
        }

        private void EnsureStayersContainer(GameInitiallyLoadedEventArgs ev)
        {
            EnsureStayersContainer();
        }

        private void EnsureStayersContainer()
        {
            if (!IsAlive(_stayersEntity))
            {
                Logger.WarningS("stayers", "Creating stayers container entity");
                _stayersEntity = EntityManager.SpawnEntity(null, MapCoordinates.Global);
                DebugTools.Assert(IsAlive(_stayersEntity), "Could not initialize stayers container!");
                EnsureComp<StayersComponent>(_stayersEntity);
            }
        }

        public IEnumerable<StayingComponent> EnumerateAllStayers(string tag)
        {
            return _lookup.EntityQueryDirectlyIn<StayingComponent>(_stayersEntity)
                .Where(staying => staying.StayingLocation != null && staying.StayingLocation.Tag == tag);
        }

        private string GetAreaName(IMap map)
        {
            return _displayNames.GetDisplayName(map.MapEntityUid);
        }

        public void RegisterStayer(EntityUid ent, IMap map, string tag, Vector2i? pos = null, StayingComponent? staying = null)
        {
            if (!Resolve(ent, ref staying, logMissing: false))
                staying = EnsureComp<StayingComponent>(ent);

            string areaName = GetAreaName(map);

            staying.StayingLocation = new StayingLocation(new MapIdStayerCriteria(map.Id), pos != null ? new SpecificMapLocation(pos.Value) : new MapAIAnchorLocation(), areaName, tag);
        }

        public void RegisterStayer(EntityUid ent, MapCoordinates mapCoords, string areaName, string tag, StayingComponent? staying = null)
        {
            if (!Resolve(ent, ref staying, logMissing: false))
                staying = EnsureComp<StayingComponent>(ent);

            staying.StayingLocation = new StayingLocation(new MapIdStayerCriteria(mapCoords.MapId), new SpecificMapLocation(mapCoords.Position), areaName, tag);
        }

        public void UnregisterStayer(EntityUid ent, StayingComponent? staying = null)
        {
            if (!Resolve(ent, ref staying, logMissing: false))
                staying = EnsureComp<StayingComponent>(ent);

            staying.StayingLocation = null;
        }

        private bool IsValidStayer(EntityUid ent)
        {
            return !_gameSession.IsPlayer(ent);
        }

        private void TransferFromMapToStayers(BeforeMapLeaveEventArgs ev)
        {
            EnsureStayersContainer();

            var container = _stayersContainer;

            foreach (var staying in _lookup.EntityQueryInMap<StayingComponent>(ev.OldMap).Where(staying => staying.StayingLocation != null && !staying.StayingLocation.Criteria.CanAppear(staying.Owner, ev.NewMap)).ToList())
            {
                if (IsValidStayer(staying.Owner))
                {
                    Logger.DebugS("stayers", $"Moving stayer into stayers container: {staying.Owner}");
                    container.Insert(staying.Owner);
                }
                else
                {
                    Logger.ErrorS("stayers", $"Invalid stayer! {staying.Owner}");
                }
            }
        }

        private void TransferFromStayersToMap(AfterMapEnterEventArgs ev)
        {
            EnsureStayersContainer();

            foreach (var (staying, spatial) in _lookup.EntityQueryDirectlyIn<StayingComponent, SpatialComponent>(_stayersEntity).Where(pair => pair.Item1.StayingLocation != null && pair.Item1.StayingLocation.Criteria.CanAppear(pair.Item1.Owner, ev.NewMap)).ToList())
            {
                IMapStartLocation startLoc;
                if (staying.StayingLocation!.Location != null)
                {
                    startLoc = staying.StayingLocation.Location;
                }
                else
                {
                    startLoc = new CenterMapLocation();
                }

                var pos = startLoc.GetStartPosition(staying.Owner, ev.NewMap);
                Logger.DebugS("stayers", $"Moving stayer to map: {staying.Owner} -> {pos}");

                if (!_mapPlacement.TryPlaceChara(spatial.Owner, ev.NewMap.AtPos(pos)))
                    Logger.WarningS("stayers", $"Could not restore stayer! {spatial}");
            }
        }
    }
}