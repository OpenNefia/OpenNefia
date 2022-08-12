using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maps;
using OpenNefia.Content.UI;
using OpenNefia.Content.Currency;

namespace OpenNefia.Content.Activity
{
    public sealed partial class VanillaActivitiesSystem
    {
        private void Initialize_Sex()
        {
            SubscribeComponent<ActivitySexComponent, OnActivityStartEvent>(Sex_OnStart);
            SubscribeComponent<ActivitySexComponent, OnActivityPassTurnEvent>(Sex_OnPassTurn);
            SubscribeComponent<ActivitySexComponent, OnActivityFinishEvent>(Sex_OnFinish);
        }

        private void Sex_OnStart(EntityUid activity, ActivitySexComponent component, OnActivityStartEvent args)
        {
            if (!IsAlive(component.Partner))
            {
                args.Cancel();
                return;
            }

            if (component.IsTopping)
            {
                _mes.Display(Loc.GetString("Elona.Activity.Sex.TakesClothesOff", ("chara", args.Activity.Actor)), entity: args.Activity.Actor);
                var partnerSex = EntityManager.SpawnEntity(Protos.Activity.Sex, MapCoordinates.Global);
                var partnerSexComp = Comp<ActivitySexComponent>(partnerSex);
                partnerSexComp.Partner = args.Activity.Actor;
                partnerSexComp.IsTopping = false;
                _activities.StartActivity(component.Partner, partnerSex, turns: args.Activity.TurnsRemaining * 2);
            }
        }

        private bool TryEndSex(EntityUid top, EntityUid bottom)
        {
            if (!IsAlive(bottom) || !_activities.HasActivity(bottom, Protos.Activity.Sex))
            {
                _mes.Display(Loc.GetString("Elona.Activity.Sex.SpareLife", ("actor", top), ("partner", bottom)));
                _activities.RemoveActivity(top);
                _activities.RemoveActivity(bottom);
                return true;
            }

            if (!IsAlive(top))
            {
                _activities.RemoveActivity(top);
                _activities.RemoveActivity(bottom);
                return true;
            }

            if (_gameSession.IsPlayer(top))
            {
                if (!_damage.DoStaminaCheck(top, 1 + _rand.Next(2)))
                {
                    _mes.Display("Elona.Common.TooExhausted");
                    _activities.RemoveActivity(top);
                    _activities.RemoveActivity(bottom);
                    return true;
                }
            }

            return false;
        }

        private void Sex_OnPassTurn(EntityUid activity, ActivitySexComponent component, OnActivityPassTurnEvent args)
        {
            if (TryEndSex(args.Activity.Actor, component.Partner))
                return;

            if (!component.IsTopping && args.Activity.TurnsRemaining % 5 == 0)
            {
                _mes.Display(Loc.GetString("Elona.Activity.Sex.Dialog"), UiColors.MesSkyBlue, entity: args.Activity.Actor);
            }
        }

        private void ApplySexEffects(EntityUid entity, bool isBottom)
        {
            _effects.Remove(entity, Protos.StatusEffect.Drunk);

            var attrExp = 250;
            if (!_parties.IsUnderlingOfPlayer(entity))
                attrExp += 1000;

            if (isBottom)
            {
                if (_rand.OneIn(3))
                    _effects.Apply(entity, Protos.StatusEffect.Insanity, 500);
                if (_rand.OneIn(5))
                    _effects.Apply(entity, Protos.StatusEffect.Paralysis, 500);
                _effects.Apply(entity, Protos.StatusEffect.Insanity, 300);
                _sanity.HealInsanity(entity, 10);
                _skills.GainSkillExp(entity, Protos.Skill.AttrConstitution, attrExp);
                _skills.GainSkillExp(entity, Protos.Skill.AttrWill, attrExp);
            }

            if (_rand.OneIn(15))
                _effects.Apply(entity, Protos.StatusEffect.Sick, 200);

            _skills.GainSkillExp(entity, Protos.Skill.AttrCharisma, attrExp);
        }

        private int CalcSexGoldEarned(EntityUid actor)
        {
            return _skills.Level(actor, Protos.Skill.AttrCharisma) * (50 + _rand.Next(50)) + 100;
        }

        private void Sex_OnFinish(EntityUid activity, ActivitySexComponent component, OnActivityFinishEvent args)
        {
            if (TryEndSex(args.Activity.Actor, component.Partner))
                return;

            if (!component.IsTopping)
                return;

            var actor = args.Activity.Actor;

            ApplySexEffects(actor, isBottom: false);
            ApplySexEffects(component.Partner, isBottom: true);

            int goldEarned = CalcSexGoldEarned(actor);

            var mes = Loc.GetString("Elona.Activity.Sex.DialogAfter", ("partner", component.Partner));

            if (!_gameSession.IsPlayer(component.Partner))
            {
                if (TryComp<MoneyComponent>(component.Partner, out var partnerWallet))
                {
                    if (partnerWallet.Gold >= goldEarned)
                    {
                        mes += Loc.Space + Loc.GetString("Elona.Activity.Sex.Take", ("partner", component.Partner));
                    }
                    else
                    {
                        if (_vis.IsInWindowFov(actor))
                        {
                            mes += Loc.Space + Loc.GetString("Elona.Activity.Sex.TakeAllIHave", ("partner", component.Partner));
                            if (_rand.OneIn(3) && !_gameSession.IsPlayer(actor))
                            {
                                _mes.Display(Loc.GetString("Elona.Activity.Sex.GetsFurious", ("actor", actor)));
                                _vanillaAI.SetTarget(actor, component.Partner, 20);
                            }
                        }
                        partnerWallet.Gold = Math.Max(partnerWallet.Gold, 1);
                        goldEarned = partnerWallet.Gold;
                    }

                    partnerWallet.Gold -= goldEarned;

                    if (_gameSession.IsPlayer(actor))
                    {
                        _dialog.ModifyImpression(component.Partner, 5);
                        _itemGen.GenerateItem(actor, Protos.Item.GoldPiece, amount: goldEarned);
                        _mes.Display(Loc.GetString("Elona.Common.SomethingIsPut"));
                        _karma.ModifyKarma(actor, -1);
                    }
                    else if (TryComp<MoneyComponent>(actor, out var actorWallet))
                    {
                        actorWallet.Gold += goldEarned;
                    }
                }
            }

            _mes.Display(Loc.GetString("Elona.Common.Quotes", ("str", mes)), UiColors.MesTalk, entity: actor);

            _activities.RemoveActivity(component.Partner);
        }
    }
}