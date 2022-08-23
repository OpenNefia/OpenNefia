using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Configuration;
using OpenNefia.Content.Items;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.Equipment;

namespace OpenNefia.Content.Enchantments
{
    public sealed partial class EnchantmentGenSystem
    {
        [Dependency] private readonly IConfigurationManager _config = default!;

        public override void Initialize()
        {
            SubscribeComponent<EquipmentComponent, EntityBeingGeneratedEvent>(Equipment_AddRandomEnchantments, priority: EventPriorities.VeryLow);
            SubscribeComponent<WeaponComponent, OnAddRandomEnchantmentsEventArgs>(AddStaffEnchantments);
            SubscribeEntity<OnAddRandomEnchantmentsEventArgs>(AddEgoEnchantments);
            SubscribeEntity<OnAddRandomEnchantmentsEventArgs>(AddQualityBasedEnchantments);
            SubscribeComponent<CurseStateComponent, OnAddRandomEnchantmentsEventArgs>(AddCurseStateBasedEnchantments);
        }

        private void Equipment_AddRandomEnchantments(EntityUid item, EquipmentComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (!HasComp<ItemComponent>(item))
                return;
            
            // >>>>>>>> elona122/shade2/item.hsp:541 	if refType<fltPotion{ ...
            AddRandomEnchantments(item);
            // <<<<<<<< elona122/shade2/item.hsp:543 		}else{ ...
        }

        private void AddStaffEnchantments(EntityUid item, WeaponComponent component, OnAddRandomEnchantmentsEventArgs args)
        {
            // >>>>>>>> shade2/item_data.hsp:770 	if refTypeMinor=fltStave{ ...
            if (!_tags.HasTag(item, Protos.Tag.ItemCatEquipMeleeStaff))
                return;

            EntityUid? enc = null;

            for (var i = 0; i < 1; i++)
            {
                if (_rand.OneIn(10))
                {
                    _enchantments.AddEnchantment(item, Protos.Enchantment.EnhanceSpells, CalcRandomEnchantmentPower());
                }
                if (_rand.OneIn(10))
                {
                    if (_enchantments.TryAddEnchantment(item, Protos.Enchantment.ModifyAttribute, CalcRandomEnchantmentPower(), out enc, randomize: false))
                    {
                        var encModifyAttb = Comp<EncModifyAttributeComponent>(enc.Value);
                        encModifyAttb.SkillID = Protos.Skill.AttrMagic;
                    }
                }
                if (_rand.OneIn(10))
                {
                    if (_enchantments.TryAddEnchantment(item, Protos.Enchantment.ModifySkill, CalcRandomEnchantmentPower(), out enc, randomize: false))
                    {
                        var encModifySkill = Comp<EncModifySkillComponent>(enc.Value);
                        encModifySkill.SkillID = Protos.Skill.Casting;
                        break;
                    }
                }
                if (_rand.OneIn(10))
                {
                    if (_enchantments.TryAddEnchantment(item, Protos.Enchantment.ModifyAttribute, CalcRandomEnchantmentPower(), out enc, randomize: false))
                    {
                        var encModifyAttb = Comp<EncModifyAttributeComponent>(enc.Value);
                        encModifyAttb.SkillID = Protos.Skill.AttrMana;
                        break;
                    }
                }
                if (_rand.OneIn(10))
                {
                    if (_enchantments.TryAddEnchantment(item, Protos.Enchantment.ModifySkill, CalcRandomEnchantmentPower(), out enc, randomize: false))
                    {
                        var encModifySkill = Comp<EncModifySkillComponent>(enc.Value);
                        encModifySkill.SkillID = Protos.Skill.MagicCapacity;
                        break;
                    }
                }
            }
            // <<<<<<<< shade2/item_data.hsp:778 		} ..
        }

        private void AddEgoEnchantments(EntityUid item, OnAddRandomEnchantmentsEventArgs args)
        {
            if (args.ObjectQuality < Quality.Great)
            {
                if (_rand.OneIn(2))
                {
                    AddEgoMinorEnchantments(item, args.EgoLevel);
                }
                else
                {
                    AddEgoMajorEnchantments(item, args.EgoLevel);
                }
            }
        }

        private void AddGreatAndGodlyEnchantments(EntityUid item, OnAddRandomEnchantmentsEventArgs args)
        {
            var quality = args.ObjectQuality;

            if (quality == Quality.God || (quality == Quality.Great && _rand.OneIn(10)))
            {
                GenerateFixedLevelEnchantment(item, 99);
            }

            int encCount;
            if (quality == Quality.God)
            {
                encCount = _rand.Next(_rand.Next(_rand.Next(10) + 1) + 3) + 6;
            }
            else
            {
                encCount = _rand.Next(_rand.Next(_rand.Next(10) + 1) + 3) + 3;
            }
            if (_config.GetCVar(CCVars.DebugDevelopmentMode) && quality == Quality.God)
            {
                encCount = 12;
            }

            if (encCount > 11 && HasComp<WeaponComponent>(item) && _rand.OneIn(10))
            {
                var eternalForce = EnsureComp<EternalForceComponent>(item);
                eternalForce.IsEternalForce.Base = true;

                GenerateFixedLevelEnchantment(item, 99);

                if (TryComp<CurseStateComponent>(item, out var curseState))
                    curseState.CurseState = CurseState.Blessed;
            }

            for (var i = 0; i < encCount; i++)
            {
                var level = CalcRandomEnchantmentLevel(args.EgoLevel);
                var id = PickRandomEnchantmentID(item, level);
                if (id == null)
                    continue;

                var power = CalcRandomEnchantmentPower();
                var isEternalForce = CompOrNull<EternalForceComponent>(item)?.IsEternalForce.Buffed ?? false;
                if (quality == Quality.God)
                {
                    power += 100;
                }
                if (isEternalForce)
                {
                    power += 100;
                }

                var cursePower = 20;
                if (quality == Quality.God)
                {
                    cursePower -= 10;
                }
                if (isEternalForce)
                {
                    cursePower -= 20;
                }

                _enchantments.AddEnchantment(item, id.Value, power, cursePower);
            }
        }

        private void AddQualityBasedEnchantments(EntityUid item, OnAddRandomEnchantmentsEventArgs args)
        {
            var quality = args.ObjectQuality;

            if (quality == Quality.Great || quality == Quality.God)
            {
                AddGreatAndGodlyEnchantments(item, args);
            }

            if (quality == Quality.Unique)
            {
                var count = _rand.Next(3);
                for (var i = 0; i < count; i++)
                {
                    GenerateEnchantment(item, args.EgoLevel, cursePower: 10);
                }
            }
        }

        private void AddCurseStateBasedEnchantments(EntityUid item, CurseStateComponent curse, OnAddRandomEnchantmentsEventArgs args)
        {
            if (curse.CurseState <= CurseState.Cursed)
            {
                var level = CalcRandomEnchantmentLevel(args.EgoLevel);
                var id = PickRandomEnchantmentID(item, level);
                if (id != null)
                {
                    var power = CalcRandomEnchantmentPower();
                    power = Math.Clamp(power, 250, 10000) * (125 + (curse.CurseState == CurseState.Doomed ? 25 : 0)) / 100;

                    _enchantments.AddEnchantment(item, id.Value, power);
                }

                var encCount = 1 + _rand.Next(2);
                if (curse.CurseState == CurseState.Doomed)
                    encCount++;

                for (var i = 0; i < encCount; i++)
                {
                    if (_rand.OneIn(3))
                    {
                        var power = CalcRandomEnchantmentPower() * 3 / 2;
                        _enchantments.AddEnchantment(item, Protos.Enchantment.ModifyResistance, power);
                    }
                    else if (_rand.OneIn(3))
                    {
                        var power = CalcRandomEnchantmentPower() * 5 / 2;
                        _enchantments.AddEnchantment(item, Protos.Enchantment.ModifyAttribute, power);
                    }
                    else
                    {
                        GenerateFixedLevelEnchantment(item, -1);
                    }
                }
            }
        }
    }
}
