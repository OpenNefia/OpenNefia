using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Items;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using static OpenNefia.Content.Prototypes.Protos;
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
using MaterialPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Materials.MaterialPrototype>;
using OpenNefia.Content.Levels;
using OpenNefia.Content.SenseQuality;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.CharaMake;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Weight;
using OpenNefia.Content.Equipment;
using YamlDotNet.Core.Tokens;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Identify;

namespace OpenNefia.Content.Materials
{
    public interface IMaterialSystem : IEntitySystem
    {
        MaterialPrototypeId PickRandomMaterialID(EntityUid uid, MaterialPrototypeId? baseMaterial = null);
        MaterialPrototypeId PickRandomMaterialIDRaw(int matQualityIndex, Quality baseQuality, MaterialPrototypeId? baseMaterial = null, EntityUid? item = null);

        int GetMaterialStatDivisor(Quality quality);

        void ChangeItemMaterial(EntityUid item, MaterialPrototypeId materialID, MaterialComponent? materialComp = null);
    }

    public sealed class MaterialSystem : EntitySystem, IMaterialSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IQualitySystem _qualities = default!;
        [Dependency] private readonly ICharaMakeLogic _charaMakeLogic = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly IIdentifySystem _identify = default!;

        public override void Initialize()
        {
            SubscribeComponent<MaterialComponent, EntityBeingGeneratedEvent>(Material_BeingGenerated, priority: EventPriorities.High);
            SubscribeComponent<MaterialComponent, GetItemDescriptionEventArgs>(Material_GetItemDescription);
            SubscribeComponent<MaterialComponent, EntityRefreshEvent>(Material_Refreshed, priority: EventPriorities.VeryHigh);
            SubscribeComponent<MaterialComponent, GetItemDescriptionEventArgs>(Material_GetItemDescription, priority: EventPriorities.VeryHigh);

            SubscribeComponent<WeightComponent, EntityApplyMaterialEvent>(Weight_ApplyMaterial);
            SubscribeComponent<ValueComponent, EntityApplyMaterialEvent>(Value_ApplyMaterial);
            SubscribeComponent<ChipComponent, EntityApplyMaterialEvent>(Chip_ApplyMaterial);
            SubscribeComponent<EquipStatsComponent, EntityApplyMaterialEvent>(EquipStats_ApplyMaterial);
            SubscribeComponent<WeaponComponent, EntityApplyMaterialEvent>(Weapon_ApplyMaterial);
            SubscribeComponent<AmmoComponent, EntityApplyMaterialEvent>(Ammo_ApplyMaterial);
        }

        private bool ShouldRandomizeMaterial(EntityUid uid)
        {
            return HasComp<EquipmentComponent>(uid) || (HasComp<FurnitureComponent>(uid) && _rand.OneIn(5));
        }

        private bool IsRandomizedMaterialType(MaterialPrototypeId? materialID)
        {
            return materialID != null && RandomMaterialTables.ContainsKey(materialID.Value);
        }

        private void Material_BeingGenerated(EntityUid uid, MaterialComponent material, ref EntityBeingGeneratedEvent args)
        {
            material.RandomSeed = _rand.Next();

            if (ShouldRandomizeMaterial(uid))
            {
                if (IsRandomizedMaterialType(material.MaterialID) || HasComp<FurnitureComponent>(uid))
                {
                    var matId = PickRandomMaterialID(uid);
                    material.MaterialID = matId;
                }
                else if (material.MaterialID != null)
                {
                    // If an item is generated with a material already defined on it (like the Blood
                    // Moon), then the stat bonuses from the material will *not* be applied, but the
                    // enchantments will.
                    // See shade2/item.hsp:531.
                    ApplyMaterialEnchantments(uid, material);
                }
            }
        }

        private void Material_GetItemDescription(EntityUid uid, MaterialComponent material, GetItemDescriptionEventArgs args)
        {
            if (_identify.GetIdentifyState(uid) < IdentifyState.Quality || material.MaterialID == null)
                return;

            var materialName = Loc.GetPrototypeString(material.MaterialID.Value, "Name");
            var entry = new ItemDescriptionEntry()
            {
                Text = Loc.GetString("Elona.ItemDescription.ItIsMadeOf", ("materialName", materialName))
            };
            args.OutEntries.Add(entry);
        }

        private void Material_Refreshed(EntityUid uid, MaterialComponent component, ref EntityRefreshEvent args)
        {
            if (component.MaterialID == null)
                return;

            var matProto = _protos.Index(component.MaterialID.Value);
            var ev = new EntityApplyMaterialEvent(matProto, component.RandomSeed);
            RaiseEvent(uid, ref ev);
        }

        private void Material_GetItemDescription(EntityUid uid, MaterialComponent material, GetItemDescriptionEventArgs args)
        {
            if (_identify.GetIdentifyState(uid) >= IdentifyState.Quality && material.MaterialID != null)
            {
                var materialName = Loc.GetPrototypeString(material.MaterialID.Value, "Name");
                var entry = new ItemDescriptionEntry()
                {
                    Text = Loc.GetString("Elona.ItemDescription.ItIsMadeOf", ("materialName", materialName))
                };
                args.OutEntries.Add(entry);
            }
        }

        private void Weight_ApplyMaterial(EntityUid uid, WeightComponent weight, ref EntityApplyMaterialEvent args)
        {
            weight.Weight.Buffed = (int)(weight.Weight.Buffed * args.MaterialProto.WeightModifier);
        }

        private void Value_ApplyMaterial(EntityUid uid, ValueComponent value, ref EntityApplyMaterialEvent args)
        {
            if (TryComp<FurnitureComponent>(uid, out var furnitureComp))
            {
                value.Value.Buffed = value.Value.Buffed + (int)(args.MaterialProto.ValueModifier * 200);
                if (furnitureComp.FurnitureQuality > 0)
                {
                    value.Value.Buffed = (int)(value.Value.Buffed * (0.8 + furnitureComp.FurnitureQuality * 0.2));
                }
            }
            else
            {
                value.Value.Buffed = (int)(value.Value.Buffed * args.MaterialProto.ValueModifier);
            }
        }

        private void Chip_ApplyMaterial(EntityUid uid, ChipComponent chip, ref EntityApplyMaterialEvent args)
        {
            chip.Color = args.MaterialProto.Color;
        }

        private void EquipStats_ApplyMaterial(EntityUid uid, EquipStatsComponent equipment, ref EntityApplyMaterialEvent args)
        {
            var quality = _qualities.GetQuality(uid);
            var statDivisor = GetMaterialStatDivisor(quality);

            var materialProto = args.MaterialProto;
            _rand.WithSeed(args.RandomSeed + 1000, DoApply);

            void DoApply()
            {
                if (equipment.HitBonus.Buffed > 0)
                    equipment.HitBonus.Buffed = materialProto.HitBonus * equipment.HitBonus.Buffed * 9 / (Math.Max(statDivisor - _rand.Next(30), 1));

                if (equipment.DamageBonus.Buffed > 0)
                    equipment.DamageBonus.Buffed = materialProto.DamageBonus * equipment.DamageBonus.Buffed * 5 / (Math.Max(statDivisor - _rand.Next(30), 1));

                if (equipment.DV.Buffed > 0)
                    equipment.DV.Buffed = materialProto.DV * equipment.DV.Buffed * 7 / (Math.Max(statDivisor - _rand.Next(30), 1));

                if (equipment.PV.Buffed > 0)
                    equipment.PV.Buffed = materialProto.PV * equipment.PV.Buffed * 9 / (Math.Max(statDivisor - _rand.Next(30), 1));
            }
        }

        private void Weapon_ApplyMaterial(EntityUid uid, WeaponComponent weapon, ref EntityApplyMaterialEvent args)
        {
            var quality = _qualities.GetQuality(uid);
            var statDivisor = GetMaterialStatDivisor(quality);

            var materialProto = args.MaterialProto;
            _rand.WithSeed(args.RandomSeed + 2000, DoApply);

            void DoApply()
            {
                if (weapon.DiceY.Buffed > 0)
                    weapon.DiceY.Buffed = materialProto.DiceY * weapon.DiceY.Buffed / (statDivisor + _rand.Next(25));
            }
        }

        private void Ammo_ApplyMaterial(EntityUid uid, AmmoComponent ammo, ref EntityApplyMaterialEvent args)
        {
            var quality = _qualities.GetQuality(uid);
            var statDivisor = GetMaterialStatDivisor(quality);

            var materialProto = args.MaterialProto;
            _rand.WithSeed(args.RandomSeed + 3000, DoApply);

            void DoApply()
            {
                if (ammo.DiceY.Buffed > 0)
                    ammo.DiceY.Buffed = materialProto.DiceY * ammo.DiceY.Buffed / (statDivisor + _rand.Next(25));
            }
        }

        public int GetMaterialStatDivisor(Quality quality)
        {
            int statDivisor;
            if (quality <= Quality.Bad)
                statDivisor = 150;
            else if (quality == Quality.Normal)
                statDivisor = 100;
            else if (quality >= Quality.Good)
                statDivisor = 80;
            else
                statDivisor = 120;
            return statDivisor;
        }

        private const int MaxMaterialRarities = 5;
        private const int MaxMaterialQualities = 4;

        /// <summary>
        /// These are the global generation tables for materials. The material chosen depends on the
        /// quality of the item/material and the level of the item/character wielding it. The first
        /// dimension (material rarity) is chosen at random using a hardcoded formula. The second
        /// dimension is indexed by material quality from worst to best.
        ///
        /// To make this moddable we'd have to change the formula since it assumes a fixed 5x4 array
        /// of possible materials to choose from.
        /// </summary>
        private static readonly Dictionary<MaterialPrototypeId, MaterialPrototypeId[,]> RandomMaterialTables = new()
        {
            { Material.Metal, new MaterialPrototypeId[MaxMaterialRarities,MaxMaterialQualities] {
               { Material.Bronze, Material.Lead, Material.Mica, Material.Coral },
               { Material.Iron, Material.Silver, Material.Glass, Material.Obsidian },
               { Material.Steel, Material.Platinum, Material.Pearl, Material.Mithril },
               { Material.Chrome, Material.Crystal, Material.Emerald, Material.Adamantium },
               { Material.Titanium, Material.Diamond, Material.Rubynus, Material.Ether },
            }
            },
            { Material.Soft, new MaterialPrototypeId[MaxMaterialRarities,MaxMaterialQualities] {
               { Material.Cloth, Material.Silk, Material.Paper, Material.Bone },
               { Material.Leather, Material.Scale, Material.Glass, Material.Obsidian },
               { Material.Chain, Material.Platinum, Material.Pearl, Material.Mithril },
               { Material.Zylon, Material.Gold, Material.SpiritCloth, Material.DragonScale },
               { Material.DawnCloth, Material.GriffonScale, Material.Rubynus, Material.Ether },
            }
            }
        };

        public MaterialPrototypeId PickRandomMaterialID(EntityUid uid, MaterialPrototypeId? baseMaterial = null)
        {
            var baseLevel = _levels.GetLevel(uid);
            var baseQuality = _qualities.GetQuality(uid);

            int level;

            // TODO would be very nice to have information about entities being generated further up the stack.
            // In this case I would like the level of the character this item is being generated onto, if any.
            if (_charaMakeLogic.CharaMakeIsActive)
                level = 1;
            else
                level = _rand.Next(baseLevel + 1) / 10 + 1;

            return PickRandomMaterialIDRaw(level, baseQuality, baseMaterial, uid);
        }

        public MaterialPrototypeId PickRandomMaterialIDRaw(int matQualityIndex, Quality baseQuality, MaterialPrototypeId? baseMaterial = null, EntityUid? item = null)
        {
            MaterialPrototypeId material;
            if (baseMaterial == null)
            {
                if (IsAlive(item) && TryComp<MaterialComponent>(item, out var matComp) && matComp.MaterialID != null)
                    material = matComp.MaterialID.Value;
                else
                    material = Material.Sand;
            }
            else
            {
                material = baseMaterial.Value;
            }

            int matRarityIndex;

            if (_charaMakeLogic.CharaMakeIsActive)
            {
                matQualityIndex = 0;
                matRarityIndex = 0;
            }
            else
            {
                var i = _rand.Next(100);
                if (i < 5)
                    matRarityIndex = 3;
                else if (i < 25)
                    matRarityIndex = 2;
                else if (i < 55)
                    matRarityIndex = 1;
                else
                    matRarityIndex = 0;
            }

            matQualityIndex = Math.Clamp(_rand.Next(matQualityIndex + 1) + (int)baseQuality, 0, MaxMaterialQualities);

            if (IsAlive(item) && HasComp<FurnitureComponent>(item.Value))
            {
                if (_rand.OneIn(2))
                    material = Material.Metal;
                else
                    material = Material.Soft;
            }

            if (material == Material.Metal)
            {
                if (_rand.OneIn(10))
                    material = Material.Soft;
            }
            else if (material == Material.Soft)
            {
                if (_rand.OneIn(10))
                    material = Material.Metal;
            }

            if (RandomMaterialTables.TryGetValue(material, out var table))
            {
                material = table[matQualityIndex, matRarityIndex];
            }

            if (_rand.OneIn(25))
                material = Material.Fresh;

            if (IsAlive(item) && HasComp<FurnitureComponent>(item.Value) && _protos.TryIndex(material, out var materialProto))
            {
                if (!materialProto.GenerateOnFurniture)
                    material = Material.Wood;
            }

            return material;
        }

        public void ChangeItemMaterial(EntityUid item, MaterialPrototypeId materialID, MaterialComponent? materialComp = null)
        {
            if (!Resolve(item, ref materialComp))
                return;

            materialComp.RandomSeed = _rand.Next();
            materialComp.MaterialID = materialID;
            _refresh.Refresh(item);
        }

        private void ApplyMaterialEnchantments(EntityUid uid, MaterialComponent? material = null)
        {
            if (!Resolve(uid, ref material))
                return;

            // TODO enchantments
        }
    }

    [ByRefEvent]
    [EventUsage(EventTarget.Normal)]
    public struct EntityApplyMaterialEvent
    {
        public MaterialPrototype MaterialProto { get; }
        public int RandomSeed { get; }

        public EntityApplyMaterialEvent(MaterialPrototype materialProto, int randomSeed)
        {
            MaterialProto = materialProto;
            RandomSeed = randomSeed;
        }
    }
}