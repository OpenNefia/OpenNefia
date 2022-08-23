using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Food;
using OpenNefia.Core.Game;
using OpenNefia.Content.Resists;
using OpenNefia.Content.UI;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Spells;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Logic;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Locale;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.Levels;

namespace OpenNefia.Content.Enchantments
{
    public sealed partial class VanillaEnchantmentsSystem
    {
        [Dependency] private readonly IStatusEffectSystem _statusEffects = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;

        private void Initialize_Unique()
        {
            SubscribeComponent<EncRandomTeleportComponent, CalcEnchantmentAdjustedPowerEvent>(EncRandomTeleport_CalcAdjustedPower);
            SubscribeComponent<EncRandomTeleportComponent, ApplyEnchantmentAfterPassTurnEvent>(EncRandomTeleport_ApplyAfterPassTurn);

            SubscribeComponent<EncSuckBloodComponent, CalcEnchantmentAdjustedPowerEvent>(EncSuckBlood_CalcAdjustedPower);
            SubscribeComponent<EncSuckBloodComponent, ApplyEnchantmentAfterPassTurnEvent>(EncSuckBlood_ApplyAfterPassTurn);

            SubscribeComponent<EncSuckExperienceComponent, CalcEnchantmentAdjustedPowerEvent>(EncSuckExperience_CalcAdjustedPower);
            SubscribeComponent<EncSuckExperienceComponent, ApplyEnchantmentAfterPassTurnEvent>(EncSuckExperience_ApplyAfterPassTurn);

            SubscribeComponent<EncSummonCreatureComponent, CalcEnchantmentAdjustedPowerEvent>(EncSummonCreature_CalcAdjustedPower);
            SubscribeComponent<EncSummonCreatureComponent, ApplyEnchantmentAfterPassTurnEvent>(EncSummonCreature_ApplyAfterPassTurn);

            SubscribeComponent<EncResistBlindness, ApplyEnchantmentOnRefreshEvent>(EncResistBlindness_ApplyOnRefresh);
        }

        private void EncRandomTeleport_CalcAdjustedPower(EntityUid uid, EncRandomTeleportComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncRandomTeleport_ApplyAfterPassTurn(EntityUid uid, EncRandomTeleportComponent component, ref ApplyEnchantmentAfterPassTurnEvent args)
        {
            if (!TryMap(uid, out var map) || HasComp<MapTypeWorldMapComponent>(map.MapEntityUid))
                return;

            if (_rand.Next(25) < Math.Clamp(Math.Abs(args.AdjustedPower) / 50, 1, 25))
            {
                _spells.Cast(Protos.Spell.SpellTeleport, args.AdjustedPower, target: args.Equipper, source: args.Equipper);
            }
        }


        private void EncSuckBlood_CalcAdjustedPower(EntityUid uid, EncSuckBloodComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncSuckBlood_ApplyAfterPassTurn(EntityUid uid, EncSuckBloodComponent component, ref ApplyEnchantmentAfterPassTurnEvent args)
        {
            if (_rand.OneIn(4))
            {
                _mes.Display(Loc.GetString("Elona.Enchantment.Item.SuckBlood.BloodSucked", ("entity", args.Equipper)), color: UiColors.MesPurple, entity: args.Equipper);
                var bleedPower = Math.Abs(args.TotalPower) / 25 + 3;
                _statusEffects.Apply(args.Equipper, Protos.StatusEffect.Bleeding, bleedPower);
            }
        }

        private void EncSuckExperience_CalcAdjustedPower(EntityUid uid, EncSuckExperienceComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncSuckExperience_ApplyAfterPassTurn(EntityUid uid, EncSuckExperienceComponent component, ref ApplyEnchantmentAfterPassTurnEvent args)
        {
            if (_rand.OneIn(4) && TryComp<LevelComponent>(uid, out var level))
            {
                _mes.Display(Loc.GetString("Elona.Enchantment.Item.SuckExperience.ExperienceReduced", ("entity", args.Equipper)), color: UiColors.MesPurple, entity: args.Equipper);
                var lostExp = level.ExperienceToNext / (100 - Math.Clamp(Math.Abs(args.TotalPower) / 2, 0, 50)) + _rand.Next(100);
                level.Experience = Math.Max(level.Experience - lostExp, 0);
            }
        }

        private void EncSummonCreature_CalcAdjustedPower(EntityUid uid, EncSummonCreatureComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncSummonCreature_ApplyAfterPassTurn(EntityUid uid, EncSummonCreatureComponent component, ref ApplyEnchantmentAfterPassTurnEvent args)
        {
            if (!TryMap(args.Equipper, out var map) || HasComp<MapTypeWorldMapComponent>(map.MapEntityUid) || HasComp<MapTypePlayerOwnedComponent>(map.MapEntityUid))
            {
                return;
            }

            if (_rand.Next(50) < Math.Clamp(Math.Abs(args.AdjustedPower), 1, 50))
            {
                _mes.Display(Loc.GetString("Elona.Enchantment.Item.SummonCreature.CreatureSummoned", ("entity", args.Equipper)), color: UiColors.MesPurple, entity: args.Equipper);
                for (var i = 0; i < 3; i++)
                {
                    var filter = new CharaFilter()
                    {
                        MinLevel = _randomGen.CalcObjectLevel(_levels.GetLevel(_gameSession.Player) * 3 / 2 + 3),
                        Quality = _randomGen.CalcObjectQuality(Qualities.Quality.Normal)
                    };
                    _charaGen.GenerateChara(args.Equipper, filter);
                }
            }
        }

        private void EncResistBlindness_ApplyOnRefresh(EntityUid uid, EncResistBlindness component, ApplyEnchantmentOnRefreshEvent args)
        {
            _statusEffects.AddTemporaryEffectImmunity(uid, Protos.StatusEffect.Blindness);
        }
    }
}
