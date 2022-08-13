using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Pickable;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Currency;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Qualities;

namespace OpenNefia.Content.Money
{
    public interface IMoneySystem : IEntitySystem
    {
    }

    public sealed class MoneySystem : EntitySystem, IMoneySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;

        public override void Initialize()
        {
            SubscribeComponent<GoldPieceComponent, EntityBeingGeneratedEvent>(BeingGenerated_GoldPiece);
            SubscribeComponent<GoldPieceComponent, EntityGeneratedEvent>(Generated_GoldPiece, priority: EventPriorities.Lowest);
            SubscribeComponent<GoldPieceComponent, GetVerbsEventArgs>(GetVerbs_GoldPiece);

            SubscribeComponent<PlatinumCoinComponent, GetVerbsEventArgs>(GetVerbs_PlatinumCoin);
            SubscribeComponent<PlatinumCoinComponent, EntityGeneratedEvent>(Generated_PlatinumCoin, priority: EventPriorities.Lowest);
        }

        private int CalcInitialGold(EntityUid item, EntityCoordinates coords)
        {
            // >>>>>>>> shade2/calculation.hsp:854 #defcfunc calcInitGold int c ..
            var owner = coords.EntityId;

            // Generate initial gold based on map level.
            if ((!IsAlive(owner) || HasComp<MapComponent>(owner)))
            {
                var map = _mapManager.ActiveMap;
                if (TryMap(owner, out var parentMap))
                    map = parentMap;

                var amount = map != null ? _levels.GetLevel(map.MapEntityUid) * 25 : 25;

                if (map != null && HasComp<MapTypeShelterComponent>(map.MapEntityUid))
                    amount = 1;

                return _rand.Next(amount + 10) + 1;
            }

            // Generate initial gold based on the owning character's stats.
            if (TryComp<MoneyComponent>(owner, out var money) && money.InitialGold != null)
            {
                return _rand.NextIntInRange(money.InitialGold.Value);
            }

            return 1;
            // <<<<<<<< shade2/calculation.hsp:863 	return rnd(cLevel(c)*25+10)+1 ...
        }

        private void BeingGenerated_GoldPiece(EntityUid uid, GoldPieceComponent component, ref EntityBeingGeneratedEvent args)
        {
            var quality = CompOrNull<QualityComponent>(uid)?.Quality.Buffed ?? Quality.Bad;
            var initialGold = args.OriginalAmount ?? CalcInitialGold(uid, args.Coords);

            if (quality == Quality.Good)
                initialGold *= 2;
            if (quality >= Quality.Great)
                initialGold *= 4;

            _stacks.SetCount(uid, initialGold);
        }

        private void GetVerbs_GoldPiece(EntityUid uid, GoldPieceComponent component, GetVerbsEventArgs args)
        {
            if (HasComp<MoneyComponent>(args.Source))
            {
                args.OutVerbs.RemoveWhere(v => v.VerbType == PickableSystem.VerbTypePickUp);
                args.OutVerbs.Add(new Verb(PickableSystem.VerbTypePickUp, "Pick Up Gold Piece", () => PickUpGoldPiece(args.Source, args.Target),
                    priority: EventPriorities.High));
            }
        }

        private void GetVerbs_PlatinumCoin(EntityUid uid, PlatinumCoinComponent component, GetVerbsEventArgs args)
        {
            if (HasComp<MoneyComponent>(args.Source))
            {
                args.OutVerbs.RemoveWhere(v => v.VerbType == PickableSystem.VerbTypePickUp);
                args.OutVerbs.Add(new Verb(PickableSystem.VerbTypePickUp, "Pick Up Platinum Coin", () => PickUpPlatinumCoin(args.Source, args.Target),
                    priority: EventPriorities.High));
            }
        }

        private void Generated_GoldPiece(EntityUid uid, GoldPieceComponent component, ref EntityGeneratedEvent args)
        {
            if (TryComp<MoneyComponent>(args.Coords.EntityId, out var money))
            {
                money.Gold += _stacks.GetCount(uid);
                EntityManager.DeleteEntity(uid);
            }
        }

        private TurnResult PickUpGoldPiece(EntityUid picker, EntityUid item)
        {
            _mes.Display(Loc.GetString("Elona.GameObjects.Pickable.PicksUp", ("entity", picker), ("target", item)));

            _audio.Play(Protos.Sound.Getgold1, picker);

            if (TryComp<MoneyComponent>(picker, out var money))
                money.Gold += _stacks.GetCount(item);

            EntityManager.DeleteEntity(item);

            return TurnResult.Succeeded;
        }

        private void Generated_PlatinumCoin(EntityUid uid, PlatinumCoinComponent component, ref EntityGeneratedEvent args)
        {
            if (TryComp<MoneyComponent>(args.Coords.EntityId, out var money))
            {
                money.Platinum += _stacks.GetCount(uid);
                EntityManager.DeleteEntity(uid);
            }
        }

        private TurnResult PickUpPlatinumCoin(EntityUid picker, EntityUid item)
        {
            _mes.Display(Loc.GetString("Elona.GameObjects.Pickable.PicksUp", ("entity", picker), ("target", item)));

            var sound = _rand.Pick(PickableSystem.GetSounds);
            _audio.Play(sound, picker);

            if (TryComp<MoneyComponent>(picker, out var money))
                money.Platinum += _stacks.GetCount(item);

            EntityManager.DeleteEntity(item);

            return TurnResult.Succeeded;
        }
    }
}