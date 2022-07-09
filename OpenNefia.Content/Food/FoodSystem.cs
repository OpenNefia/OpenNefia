using OpenNefia.Content.Activity;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Pickable;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.World;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Logic;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Food
{
    public interface IFoodSystem : IEntitySystem
    {
        bool IsAboutToRot(EntityUid ent, FoodComponent? food = null);
        bool IsCooked(EntityUid ent, FoodComponent? food = null);
        void SetRottedState(EntityUid ent, bool setImage = true, FoodComponent? food = null);
        void SpoilFoodInMap(IMap map);
        void SpoilFood(EntityUid ent, FoodComponent? food = null);
    }

    public sealed class FoodSystem : EntitySystem, IFoodSystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IPartySystem _parties = default!;

        public override void Initialize()
        {
            SubscribeComponent<EdibleComponent, GetVerbsEventArgs>(HandleGetVerbs);
            SubscribeComponent<FoodComponent, SpoilFoodEvent>(HandleSpoilFood, priority: EventPriorities.VeryLow);
            SubscribeComponent<FoodComponent, AfterItemEatenEvent>(HandleEatFood, priority: EventPriorities.VeryLow);
            SubscribeEntity<MapOnTimePassedEvent>(ProcSpoilFoodInMap, priority: EventPriorities.High);
        }
        
        private void HandleGetVerbs(EntityUid potion, EdibleComponent edibleComp, GetVerbsEventArgs args)
        {
            args.Verbs.Add(new Verb(EdibleSystem.VerbIDEat));
        }

        private void HandleSpoilFood(EntityUid uid, FoodComponent food, SpoilFoodEvent args)
        {
            if (args.Handled)
                return;

            if (TryComp<SpatialComponent>(uid, out var spatial) && spatial.Parent != null)
            {
                if (_parties.IsInPlayerParty(spatial.Parent.Owner))
                {
                    _mes.Display(Loc.GetString("Elona.Food.GetsRotten", ("food", uid)));
                }
            }

            SetRottedState(uid, setImage: true, food);
            args.Handled = true;
        }

        private void HandleEatFood(EntityUid uid, FoodComponent component, AfterItemEatenEvent args)
        {
            // TODO
        }

        private void ProcSpoilFoodInMap(EntityUid uid, ref MapOnTimePassedEvent args)
        {
            for (var i = 0; i < args.HoursPassed; i++)
                SpoilFoodInMap(args.Map);
        }

        public bool IsAboutToRot(EntityUid ent, FoodComponent? food = null)
        {
            if (!EntityManager.IsAlive(ent))
                return false;

            if (!Resolve(ent, ref food))
                return false;

            if (TryComp<ItemComponent>(ent, out var item) && item.Material != Protos.Material.Fresh)
                return false;

            if (TryComp<PickableComponent>(ent, out var pickable) && pickable.OwnState <= OwnState.NPC)
                return false;

            return !food.IsRotten && food.SpoilageDate != null && food.SpoilageDate < _world.State.GameDate;
        }

        public bool IsCooked(EntityUid ent, FoodComponent? food = null)
        {
            if (!Resolve(ent, ref food))
                return false;

            return food.FoodQuality > 0;
        }

        public void SetRottedState(EntityUid ent, bool setImage = true, FoodComponent? food = null)
        {
            if (!Resolve(ent, ref food))
                return;

            food.IsRotten = true;
            food.SpoilageDate = null;

            if (setImage && TryComp<ChipComponent>(ent, out var chip))
                chip.ChipID = Protos.Chip.ItemRottenFood;
        }

        public void SpoilFoodInMap(IMap map)
        {
            foreach (var food in _lookup.EntityQueryInMap<FoodComponent>(map))
            {
                if (IsAboutToRot(food.Owner, food))
                    SpoilFood(food.Owner, food);
            }
        }

        public void SpoilFood(EntityUid ent, FoodComponent? food = null)
        {
            if (!Resolve(ent, ref food))
                return;

            var ev = new SpoilFoodEvent();
            Raise(ent, ev);

            if (_lookup.TryGetMapPlacedIn(ent, out var map))
            {
                var spatial = Comp<SpatialComponent>(ent);
                map.RefreshTile(spatial.WorldPosition);
            }
        }
    }

    public sealed class SpoilFoodEvent : HandledEntityEventArgs
    {
    }
}