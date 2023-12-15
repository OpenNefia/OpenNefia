using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Rendering
{
    public sealed class ScrollableSystem : EntitySystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeComponent<ScrollableComponent, BeforeMoveEventArgs>(BeforeMove_SetPrevCoords, priority: EventPriorities.VeryHigh);
            SubscribeComponent<ScrollableComponent, EntityTurnStartingEventArgs>(TurnStarting_SetPrevCoords, priority: EventPriorities.VeryHigh);
            SubscribeComponent<ScrollableComponent, PlayerTurnStartedEvent>(PlayerTurnStarting_SetPrevCoords, priority: EventPriorities.VeryHigh);
            SubscribeBroadcast<MapAfterRedrawAllEvent>(AfterRedrawAll_SetPrevCoords, priority: EventPriorities.VeryHigh);
            SubscribeComponent<ScrollableComponent, GetMapObjectMemoryEventArgs>(GetMemory_SetPrevCoords, priority: EventPriorities.VeryLow);
        }

        private void PlayerTurnStarting_SetPrevCoords(EntityUid uid, ScrollableComponent component, ref PlayerTurnStartedEvent args)
        {
            component.PreviousMapPosition = Spatial(uid).MapPosition;
        }

        private void TurnStarting_SetPrevCoords(EntityUid uid, ScrollableComponent component, EntityTurnStartingEventArgs args)
        {
            component.PreviousMapPosition = Spatial(uid).MapPosition;
        }

        private void BeforeMove_SetPrevCoords(EntityUid uid, ScrollableComponent component, BeforeMoveEventArgs args)
        {
            component.PreviousMapPosition = Spatial(uid).MapPosition;
        }

        private void AfterRedrawAll_SetPrevCoords(ref MapAfterRedrawAllEvent args)
        {
            foreach (var (spatial, scrollable) in _lookup.EntityQueryInMap<SpatialComponent, ScrollableComponent>(args.Map))
            {
                scrollable.PreviousMapPosition = spatial.MapPosition;
            }
        }

        private void GetMemory_SetPrevCoords(EntityUid uid, ScrollableComponent component, GetMapObjectMemoryEventArgs args)
        {
            args.OutMemory.PreviousCoords = component.PreviousMapPosition;
        }
    }
}