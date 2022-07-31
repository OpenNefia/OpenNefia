using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Effects;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Game;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Feats;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.DeferredEvents;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Currency;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Fame;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.Hunger;
using OpenNefia.Content.Food;
using OpenNefia.Content.Charas;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Buffs;
using OpenNefia.Content.Spells;

namespace OpenNefia.Content.RandomEvent
{
    public sealed class VanillaRandomEventsSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly ISpellSystem _spells = default!;
        [Dependency] private readonly IBuffsSystem _buffs = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IInventorySystem _inv = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly IDeferredEventsSystem _deferredEvents = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IKarmaSystem _karma = default!;
        [Dependency] private readonly IHungerSystem _hunger = default!;
        [Dependency] private readonly IFoodSystem _food = default!;
        [Dependency] private readonly ICharaSystem _charas = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;

        #region Elona.WizardsDream

        public void WizardsDream_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            _spells.Cast(Protos.Spell.EffectGainKnowledge, 100, ev.Target);
        }

        #endregion

        #region Elona.Development

        public void Development_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            _spells.Cast(Protos.Spell.EffectGainPotential, 100, ev.Target);
        }

        #endregion

        #region Elona.CreepyDream

        public void CreepyDream_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            _audio.Play(Protos.Sound.Curse2);
            _spells.Cast(Protos.Spell.EffectWeakenResistance, 100, ev.Target);
        }

        #endregion

        #region Elona.CursedWhispering

        public void CursedWhispering_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            var target = ev.Target;
            if (_feats.HasFeat(target, Protos.Feat.ResCurse))
            {
                _mes.Display(Loc.GetPrototypeString(Protos.RandomEvent.CursedWhispering, "NoEffect"));
            }
            else
            {
                if (_equipSlots.EnumerateEquippedEntities(target).Any())
                {
                    _spells.Cast(Protos.Spell.EffectCurse, 200, target, target);
                }
                else if (!_deferredEvents.IsEventQueued())
                {
                    _deferredEvents.Add(EventSleepAmbush);
                }
            }

            void EventSleepAmbush()
            {
                if (!_mapManager.TryGetMapOfEntity(target, out var map))
                    return;

                _mes.Display(Loc.GetPrototypeString(Protos.RandomEvent.CursedWhispering, "Beggars"));

                for (var i = 0; i < 3; i++)
                {
                    _charaGen.GenerateChara(target, Protos.Chara.Robber, args: EntityGenArgSet.Make(new EntityGenCommonArgs()
                    {
                        LevelOverride = _levels.GetLevel(target)
                    }));
                }
            }
        }

        #endregion

        #region Elona.Regeneration

        public void Regeneration_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            _skills.GainSkillExp(ev.Target, Protos.Skill.Healing, 1000);
        }

        #endregion

        #region Elona.Meditation

        public void Meditation_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            _skills.GainSkillExp(ev.Target, Protos.Skill.Meditation, 1000);
        }

        #endregion

        #region Elona.MaliciousHand

        public void MaliciousHand_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            if (!TryComp<WalletComponent>(ev.Target, out var wallet))
                return;

            var stolenAmount = _rand.Next(wallet.Gold / 8 + 1);
            if (CompOrNull<CommonProtectionsComponent>(ev.Target)?.IsProtectedFromTheft == true)
                stolenAmount = 0;

            if (stolenAmount > 0)
            {
                _mes.Display(Loc.GetPrototypeString(Protos.RandomEvent.MaliciousHand, "YouLose", ("stolenAmount", stolenAmount)));
                wallet.Gold -= stolenAmount;
            }
            else
            {
                _mes.Display(Loc.GetPrototypeString(Protos.RandomEvent.MaliciousHand, "NoEffect"));
            }
        }

        #endregion

        #region Elona.GreatLuck

        public void GreatLuck_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            if (TryComp<WalletComponent>(ev.Target, out var wallet))
                wallet.Platinum++;
        }

        #endregion

        #region Elona.CampingSite

        public void CampingSite_OnChoiceSelected(RandomEventPrototype proto, P_RandomEventOnChoiceSelectedEvent ev)
        {
            switch (ev.ChoiceIndex)
            {
                case 0:
                    for (var i = 0; i < 1 + _rand.Next(4); i++)
                    {
                        _itemGen.GenerateItem(ev.Target, tags: new[] { _rand.Pick(RandomGenConsts.FilterSets.Remain) });
                    }
                    _mes.Display(Loc.GetString("Elona.Common.SomethingIsPut"));
                    break;
            }
        }

        #endregion

        #region Elona.Corpse

        public void Corpse_OnChoiceSelected(RandomEventPrototype proto, P_RandomEventOnChoiceSelectedEvent ev)
        {
            switch (ev.ChoiceIndex)
            {
                case 0:
                    _mes.Display(Loc.GetPrototypeString(Protos.RandomEvent.Corpse, "Loot"));
                    _karma.ModifyKarma(ev.Target, -2);
                    for (var i = 0; i < 1 + _rand.Next(3); i++)
                    {
                        var filter = new ItemFilter()
                        {
                            Quality = _randomGen.CalcObjectQuality(Quality.Good)
                        };

                        if (_rand.OneIn(3))
                            filter.Tags = new[] { _rand.Pick(RandomGenConsts.FilterSets.Wear) };
                        else
                            filter.Tags = new[] { _rand.Pick(RandomGenConsts.FilterSets.Remain) };

                        _itemGen.GenerateItem(ev.Target, filter);
                    }
                    _mes.Display(Loc.GetString("Elona.Common.SomethingIsPut"));
                    break;
                case 1:
                    _mes.Display(Loc.GetPrototypeString(Protos.RandomEvent.Corpse, "Bury"));
                    _karma.ModifyKarma(ev.Target, 5);
                    break;
            }
        }

        #endregion

        #region Elona.SmallLuck

        public void SmallLuck_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            _spells.Cast(Protos.Spell.EffectCreateMaterial, 100, ev.Target);
        }

        #endregion

        #region Elona.SmellOfFood

        public void SmellOfFood_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            if (TryComp<HungerComponent>(ev.Target, out var hunger))
                hunger.Nutrition -= 5000;
        }

        #endregion

        #region Elona.StrangeFeast

        public void StrangeFeast_OnChoiceSelected(RandomEventPrototype proto, P_RandomEventOnChoiceSelectedEvent ev)
        {
            switch (ev.ChoiceIndex)
            {
                case 0:
                    if (TryComp<HungerComponent>(ev.Target, out var hunger))
                    {
                        hunger.Nutrition = HungerLevels.InnkeeperMeal;
                        _mes.Display(Loc.GetString("Elona.Talk.Npc.Innkeeper.Eat.Results"));
                        _mes.Display(_food.GetNutritionMessage(hunger.Nutrition));
                        _hunger.VomitIfAnorexic(ev.Target);
                    }
                    break;
            }
        }

        #endregion

        #region Elona.Murderer

        public void Murderer_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            if (!TryMap(ev.Target, out var map))
                return;

            var possibleVictims = _charas.EnumerateNonAllies(map).ToList();
            _rand.Shuffle(possibleVictims);

            foreach (var victim in possibleVictims)
            {
                if (IsAlive(victim.Owner) && TryComp<SkillsComponent>(victim.Owner, out var skills))
                {
                    _mes.Display(Loc.GetPrototypeString(Protos.RandomEvent.Murderer, "Scream"));
                    _damage.DamageHP(victim.Owner, Math.Max(skills.MaxHP, 99999), damageType: new GenericDamageType("Elona.DamageType.UnseenHand"));
                    break;
                }
            }
        }

        #endregion

        #region Elona.MadMillionaire

        public void MadMillionaire_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            if (!TryComp<WalletComponent>(ev.Target, out var wallet))
                return;

            var amount = _rand.Next(wallet.Gold / 10 + 1000) + 1;
            wallet.Gold += amount;
            _mes.Display(Loc.GetPrototypeString(Protos.RandomEvent.MadMillionaire, "YouPickUp", ("amount", amount)));
        }

        #endregion

        #region Elona.WanderingPriest

        public void WanderingPriest_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            _spells.Cast(Protos.Spell.BuffHolyVeil, 800, ev.Target);
        }

        #endregion

        #region Elona.GainingFaith

        public void GainingFaith_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            _skills.GainSkillExp(ev.Target, Protos.Skill.Faith, 1000, 6, 1000);
        }

        #endregion

        #region Elona.TreasureOfDream

        public void TreasureOfDream_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            if (!_inv.TryGetInventoryContainer(ev.Target, out var inv))
                return;
            
            var treasureMap = _itemGen.GenerateItem(inv, Protos.Item.TreasureMap);
            if (IsAlive(treasureMap))
                _mes.Display(Loc.GetString("Elona.Common.PutInBackpack", ("item", treasureMap.Value)));
        }

        #endregion

        #region Elona.LuckyDay

        public void LuckyDay_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            _buffs.AddBuff("Elona.Lucky", ev.Target, 777, 1500, ev.Target);
        }

        #endregion

        #region Elona.QuirkOfFate

        public void QuirkOfFate_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            if (!_inv.TryGetInventoryContainer(ev.Target, out var inv))
                return;

            var statue = _itemGen.GenerateItem(inv, Protos.Item.StatueOfEhekatl);
            if (IsAlive(statue))
                _mes.Display(Loc.GetString("Elona.Common.PutInBackpack", ("item", statue.Value)));
        }

        #endregion

        #region Elona.MonsterDream

        public void MonsterDream_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            _audio.Play(Protos.Sound.Curse2);
            _spells.Cast(Protos.Spell.SpellMutation, 100, ev.Target);
        }

        #endregion

        #region Elona.DreamHarvest

        public void DreamHarvest_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            _spells.Cast(Protos.Spell.EffectCreateMaterial, 200, ev.Target);
        }

        #endregion

        #region Elona.YourPotential

        public void YourPotential_OnTriggered(RandomEventPrototype proto, P_RandomEventOnTriggeredEvent ev)
        {
            _spells.Cast(Protos.Spell.EffectGainPotential, 100, ev.Target);
        }

        #endregion
    }
}