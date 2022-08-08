using OpenNefia.Content.Feats;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Karma;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.UI;
using OpenNefia.Core.Maths;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Locale;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Roles;
using OpenNefia.Content.Factions;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.Shopkeeper;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Dialog;

namespace OpenNefia.Content.Fame
{
    public interface IKarmaSystem : IEntitySystem
    {
        int GetKarma(EntityUid ent, KarmaComponent? karma = null);
        void ModifyKarma(EntityUid ent, int delta, KarmaComponent? karma = null);
        bool CaresAboutKarma(EntityUid entity);
        bool CanBeFooledByIncognito(EntityUid entity, EntityUid target);
        void TurnGuardsHostile(EntityUid uid, KarmaComponent? karma = null);
        void StartIncognito(EntityUid source);
        void EndIncognito(EntityUid source);
    }

    public sealed class KarmaSystem : EntitySystem, IKarmaSystem
    {
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IVanillaAISystem _vanillaAI = default!;
        [Dependency] private readonly IEmotionIconSystem _emoIcons = default!;
        [Dependency] private readonly IDialogSystem _dialog = default!;

        public const int KarmaThresholdBad = -30;
        public const int KarmaThresholdGood = 20;

        public override void Initialize()
        {
            SubscribeComponent<KarmaComponent, EntityRefreshEvent>(HandleRefresh);
            SubscribeEntity<CheckKillEvent>(ProcKarmaLossOnKill);
        }

        private void HandleRefresh(EntityUid uid, KarmaComponent karmaComp, ref EntityRefreshEvent args)
        {
            karmaComp.Karma.Reset();
        }

        public int GetKarma(EntityUid ent, KarmaComponent? karma = null)
        {
            if (!Resolve(ent, ref karma))
                return 0;

            return karma.Karma.Buffed;
        }

        public void ModifyKarma(EntityUid uid, int delta, KarmaComponent? karma = null)
        {
            if (!Resolve(uid, ref karma))
                return;

            if (_feats.HasFeat(uid, Protos.Feat.PermEvil) && delta < 0)
            {
                delta = (int)(delta * 0.75);
                if (delta <= 0)
                    return;
            }
            if (_feats.HasFeat(uid, Protos.Feat.PermGood))
            {
                delta = (int)(delta * 1.5);
            }

            if (delta == 0)
                return;

            Color color;
            if (delta >= 0)
                color = UiColors.MesYellow;
            else
                color = UiColors.MesPurple;

            _mes.Display(Loc.GetString("Elona.Karma.Changed", ("delta", delta)), color);

            if (delta > 0)
            {
                if (karma.Karma < KarmaThresholdBad && KarmaThresholdBad + delta >= KarmaThresholdBad)
                {
                    _mes.Display(Loc.GetString("Elona.Karma.StatusChange.NoLongerCriminal"), UiColors.MesGreen);
                }
            }
            else if (delta < 0)
            {
                if (karma.Karma >= KarmaThresholdBad && KarmaThresholdBad + delta < KarmaThresholdBad)
                {
                    _mes.Display(Loc.GetString("Elona.Karma.StatusChange.AreCriminalNow"), UiColors.MesPurple);
                    TurnGuardsHostile(uid, karma);
                }
            }
        }

        public bool CaresAboutKarma(EntityUid entity)
        {
            return EntityManager.IsAlive(entity)
                && !_parties.IsInPlayerParty(entity)
                && (HasComp<RoleGuardComponent>(entity)
                    || HasComp<RoleShopGuardComponent>(entity)
                    || IsWanderingMerchant(entity));
        }

        public bool CanBeFooledByIncognito(EntityUid entity, EntityUid target)
        {
            return EntityManager.IsAlive(entity)
                && entity != target
                && !IsWanderingMerchant(entity)
                && !HasComp<RoleShopGuardComponent>(entity)
                && _factions.GetRelationTowards(entity, target) <= Relation.Hate;
        }

        private bool IsWanderingMerchant(EntityUid entity)
        {
            return TryComp<RoleShopkeeperComponent>(entity, out var shopkeeper)
                && shopkeeper.ShopInventoryId == Protos.ShopInventory.WanderingMerchant;
        }

        private void ApplyAggro(EntityUid criminal, EntityUid guard, VanillaAIComponent? vai = null)
        {
            _factions.SetPersonalRelationTowards(guard, criminal, Relation.Enemy);
            _vanillaAI.SetTarget(guard, criminal, 80, vai);
            _emoIcons.SetEmotionIcon(guard, EmotionIcons.Angry, 2);
        }

        public void TurnGuardsHostile(EntityUid target, KarmaComponent? karma = null)
        {
            if (!Resolve(target, ref karma) || !TryMap(target, out var map))
                return;

            foreach (var vai in _lookup.EntityQueryInMap<VanillaAIComponent>(map))
            {
                var other = vai.Owner;

                if (CaresAboutKarma(other))
                {
                    ApplyAggro(target, other, vai);
                }
            }
        }

        public void StartIncognito(EntityUid target)
        {
            if (!TryMap(target, out var map))
                return;

            foreach (var vai in _lookup.EntityQueryInMap<VanillaAIComponent>(map))
            {
                var other = vai.Owner;

                if (CanBeFooledByIncognito(other, target))
                {
                    vai.Aggro = 0;
                    _factions.ClearPersonalRelationTowards(other, target);
                    _emoIcons.SetEmotionIcon(other, EmotionIcons.Question, 2);
                }
            }
        }

        public void EndIncognito(EntityUid target)
        {
            if (!TryMap(target, out var map))
                return;

            foreach (var vai in _lookup.EntityQueryInMap<VanillaAIComponent>(map))
            {
                var other = vai.Owner;

                if (other != target && HasComp<RoleGuardComponent>(target))
                {
                    ApplyAggro(target, other, vai);
                }
            }
        }

        private void ProcKarmaLossOnKill(EntityUid victim, ref CheckKillEvent args)
        {
            if (!_parties.TryGetLeader(args.Attacker, out var leader))
                return;

            if (!_parties.IsPartyLeaderOf(leader.Value, victim))
            {
                var karmaLoss = 0;

                if (_factions.GetRelationTowards(args.Attacker, victim) >= Relation.Neutral)
                    karmaLoss = -2;

                if (TryComp<KarmaValueComponent>(victim, out var karmaValue))
                    karmaLoss = -karmaValue.KarmaValue;

                // TODO generalize?
                if (HasComp<RoleShopkeeperComponent>(victim))
                    karmaLoss = -10;

                if (karmaLoss != 0)
                    ModifyKarma(args.Attacker, karmaLoss);
            }
        }
    }
}
