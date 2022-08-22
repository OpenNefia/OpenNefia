using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Logic;
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
using OpenNefia.Content.Skills;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Food;
using OpenNefia.Core.Game;

namespace OpenNefia.Content.Enchantments
{
    public sealed class VanillaEnchantmentsSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        public override void Initialize()
        {
            SubscribeComponent<EncModifyAttributeComponent, EntityBeingGeneratedEvent>(EncModifyAttribute_BeingGenerated);
            SubscribeComponent<EncModifyAttributeComponent, CalcEnchantmentAdjustedPowerEvent>(EncModifyAttribute_GetAdjustedPower);
            SubscribeComponent<EncModifyAttributeComponent, GetEnchantmentDescriptionEventArgs>(EncModifyAttribute_Localize);
            SubscribeComponent<EncModifyAttributeComponent, ApplyEnchantmentEffectsEvent>(EncModifyAttribute_Apply);
            SubscribeComponent<EncModifyAttributeComponent, ApplyEnchantmentFoodEffectsEvent>(EncModifyAttribute_ApplyAfterEaten);
        }

        #region EncModifyAttribute

        private void EncModifyAttribute_BeingGenerated(EntityUid uid, EncModifyAttributeComponent component, ref EntityBeingGeneratedEvent args)
        {
            var encArgs = args.GenArgs.Get<EnchantmentGenArgs>();
            if (!encArgs.Randomize)
                return;

            component.SkillID = _skills.PickRandomBaseAttribute().GetStrongID();
            if (encArgs.OutCursePower > 0 && _rand.Next(100) < encArgs.OutCursePower)
                encArgs.OutPower *= -2;
        }

        private void EncModifyAttribute_GetAdjustedPower(EntityUid uid, EncModifyAttributeComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncModifyAttribute_Localize(EntityUid uid, EncModifyAttributeComponent component, GetEnchantmentDescriptionEventArgs args)
        {
            args.OutGrade = args.AdjustedPower;
            args.OutShowPower = true;

            var skillName = Loc.GetPrototypeString(component.SkillID, "Name");
            if (HasComp<FoodComponent>(args.Item))
            {
                if (args.AdjustedPower < 0)
                    args.OutDescription = Loc.GetString("Elona.Enchantment.Item.ModifyAttribute.Food.Decreases", ("item", args.Item), ("skillName", skillName), ("power", args.AdjustedPower));
                else
                    args.OutDescription = Loc.GetString("Elona.Enchantment.Item.ModifyAttribute.Food.Increases", ("item", args.Item), ("skillName", skillName), ("power", args.AdjustedPower));
            }
            else
            {
                if (args.AdjustedPower < 0)
                    args.OutDescription = Loc.GetString("Elona.Enchantment.Item.ModifyAttribute.Equipment.Decreases", ("item", args.Item), ("skillName", skillName), ("power", args.AdjustedPower));
                else
                    args.OutDescription = Loc.GetString("Elona.Enchantment.Item.ModifyAttribute.Equipment.Increases", ("item", args.Item), ("skillName", skillName), ("power", args.AdjustedPower));
            }
        }

        private void EncModifyAttribute_Apply(EntityUid uid, EncModifyAttributeComponent component, ref ApplyEnchantmentEffectsEvent args)
        {
            if (_skills.TryGetKnown(args.Equipper, component.SkillID, out var skill))
                skill.Level.Buffed += args.AdjustedPower;
        }

        private void EncModifyAttribute_ApplyAfterEaten(EntityUid uid, EncModifyAttributeComponent component, ref ApplyEnchantmentFoodEffectsEvent args)
        {
            var power = args.AdjustedPower + 1;

            var exp = power * 100;
            if (!_gameSession.IsPlayer(args.Eater))
                exp *= 5;

            _skills.GainSkillExp(args.Eater, component.SkillID, exp);

            var skillName = Loc.GetPrototypeString(component.SkillID, "Name");
            if (exp < 0)
                _mes.Display(Loc.GetString("Elona.Enchantment.Item.ModifyAttribute.Eaten.Decreases", ("chara", args.Eater), ("skillName", skillName)), entity: args.Eater);
            else
                _mes.Display(Loc.GetString("Elona.Enchantment.Item.ModifyAttribute.Eaten.Increases", ("chara", args.Eater), ("skillName", skillName)), entity: args.Eater);
        }

        #endregion
    }

    [ByRefEvent]
    public struct CalcEnchantmentAdjustedPowerEvent
    {
        public int OriginalPower { get; }
        public EntityUid Item { get; }

        public int OutPower { get; set; }

        public CalcEnchantmentAdjustedPowerEvent(int power, EntityUid item)
        {
            OriginalPower = power;
            Item = item;
            OutPower = power;
        }
    }

    public sealed class GetEnchantmentDescriptionEventArgs : EntityEventArgs
    {
        public int AdjustedPower { get; }
        public EntityUid Item { get; }

        public string OutDescription { get; set; }
        public int OutGrade { get; set; }
        public bool OutShowPower { get; set; } = false;

        public GetEnchantmentDescriptionEventArgs(int adjustedPower, EntityUid item, string description)
        {
            AdjustedPower = adjustedPower;
            Item = item;
            OutDescription = description;
        }
    }

    [ByRefEvent]
    public struct ApplyEnchantmentEffectsEvent
    {
        public int AdjustedPower { get; }
        public EntityUid Equipper { get; }
        public EntityUid Item { get; }

        public ApplyEnchantmentEffectsEvent(int power, EntityUid equipper, EntityUid item)
        {
            AdjustedPower = power;
            Equipper = equipper;
            Item = item;
        }
    }

    [ByRefEvent]
    public struct ApplyEnchantmentFoodEffectsEvent
    {
        public int AdjustedPower { get; }
        public EntityUid Eater { get; }
        public EntityUid Item { get; }

        public ApplyEnchantmentFoodEffectsEvent(int power, EntityUid eater, EntityUid item)
        {
            AdjustedPower = power;
            Eater = eater;
            Item = item;
        }
    }
}