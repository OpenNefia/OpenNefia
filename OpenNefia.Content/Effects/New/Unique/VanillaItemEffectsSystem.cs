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
using OpenNefia.Content.Maps;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Parties;
using OpenNefia.Core.UserInterface;
using OpenNefia.Content.Inventory;
using OpenNefia.Core;
using OpenNefia.Content.Chargeable;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.Items;
using OpenNefia.Content.LivingWeapon;
using OpenNefia.Content.BaseAnim;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.Materials;
using OpenNefia.Content.DisplayName;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.Effects.New.Unique
{
    public sealed class VanillaItemEffectsSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IQualitySystem _qualities = default!;
        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IMaterialSystem _materials = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;

        public override void Initialize()
        {
            SubscribeComponent<EffectGainAllyComponent, ApplyEffectDamageEvent>(ApplyDamage_GainAlly);
            SubscribeComponent<EffectChangeMaterialComponent, ApplyEffectDamageEvent>(ApplyDamage_ChangeMaterial);
            //SubscribeComponent<EffectGaroksHammerComponent, ApplyEffectDamageEvent>(ApplyDamage_GaroksHammer);
            //SubscribeComponent<EffectMilkComponent, ApplyEffectDamageEvent>(ApplyDamage_Milk);
            //SubscribeComponent<EffectAleComponent, ApplyEffectDamageEvent>(ApplyDamage_Ale);
            //SubscribeComponent<EffectWaterComponent, ApplyEffectDamageEvent>(ApplyDamage_Water);
            //SubscribeComponent<EffectGainKnowledgeComponent, ApplyEffectDamageEvent>(ApplyDamage_GainKnowledge);
            //SubscribeComponent<EffectGainSkillComponent, ApplyEffectDamageEvent>(ApplyDamage_GainSkill);
            //SubscribeComponent<EffectPunishDecrementStatsComponent, ApplyEffectDamageEvent>(ApplyDamage_PunishDecrementStats);
            //SubscribeComponent<EffectGainFaithComponent, ApplyEffectDamageEvent>(ApplyDamage_GainFaith);
            //SubscribeComponent<EffectPoisonComponent, ApplyEffectDamageEvent>(ApplyDamage_Poison);
            //SubscribeComponent<EffectConfuseComponent, ApplyEffectDamageEvent>(ApplyDamage_Confuse);
            //SubscribeComponent<EffectParalyzeComponent, ApplyEffectDamageEvent>(ApplyDamage_Paralyze);
            //SubscribeComponent<EffectBlindComponent, ApplyEffectDamageEvent>(ApplyDamage_Blind);
            //SubscribeComponent<EffectSleepComponent, ApplyEffectDamageEvent>(ApplyDamage_Sleep);
            //SubscribeComponent<EffectGainPotentialComponent, ApplyEffectDamageEvent>(ApplyDamage_GainPotential);
            //SubscribeComponent<EffectCurseComponent, ApplyEffectDamageEvent>(ApplyDamage_Curse);
            //SubscribeComponent<EffectDeedComponent, ApplyEffectDamageEvent>(ApplyDamage_Deed);
            //SubscribeComponent<EffectSulfuricComponent, ApplyEffectDamageEvent>(ApplyDamage_Sulfuric);
            //SubscribeComponent<EffectCreateMaterialComponent, ApplyEffectDamageEvent>(ApplyDamage_CreateMaterial);
            //SubscribeComponent<EffectWeakenResistanceComponent, ApplyEffectDamageEvent>(ApplyDamage_WeakenResistance);
            //SubscribeComponent<EffectGainSkillPotentialComponent, ApplyEffectDamageEvent>(ApplyDamage_GainSkillPotential);
            //SubscribeComponent<EffectElixirComponent, ApplyEffectDamageEvent>(ApplyDamage_Elixir);
            //SubscribeComponent<EffectCureMutationComponent, ApplyEffectDamageEvent>(ApplyDamage_CureMutation);
            //SubscribeComponent<EffectEnchantWeaponComponent, ApplyEffectDamageEvent>(ApplyDamage_EnchantWeapon);
            //SubscribeComponent<EffectEnchantArmorComponent, ApplyEffectDamageEvent>(ApplyDamage_EnchantArmor);
            //SubscribeComponent<EffectChangeMaterialComponent, ApplyEffectDamageEvent>(ApplyDamage_ChangeMaterial);
            //SubscribeComponent<EffectDeedOfInheritanceComponent, ApplyEffectDamageEvent>(ApplyDamage_DeedOfInheritance);
            //SubscribeComponent<EffectRechargeComponent, ApplyEffectDamageEvent>(ApplyDamage_Recharge);
            //SubscribeComponent<EffectDirtyWaterComponent, ApplyEffectDamageEvent>(ApplyDamage_DirtyWater);
            //SubscribeComponent<EffectCureCorruptionComponent, ApplyEffectDamageEvent>(ApplyDamage_CureCorruption);
            //SubscribeComponent<EffectAlchemyComponent, ApplyEffectDamageEvent>(ApplyDamage_Alchemy);
            //SubscribeComponent<EffectMolotovComponent, ApplyEffectDamageEvent>(ApplyDamage_Molotov);
            //SubscribeComponent<EffectLovePotionComponent, ApplyEffectDamageEvent>(ApplyDamage_LovePotion);
            //SubscribeComponent<EffectTreasureMapComponent, ApplyEffectDamageEvent>(ApplyDamage_TreasureMap);
            //SubscribeComponent<EffectTrollBloodComponent, ApplyEffectDamageEvent>(ApplyDamage_TrollBlood);
            //SubscribeComponent<EffectFlightComponent, ApplyEffectDamageEvent>(ApplyDamage_Flight);
            //SubscribeComponent<EffectEscapeComponent, ApplyEffectDamageEvent>(ApplyDamage_Escape);
            //SubscribeComponent<EffectSaltComponent, ApplyEffectDamageEvent>(ApplyDamage_Salt);
            //SubscribeComponent<EffectDescentComponent, ApplyEffectDamageEvent>(ApplyDamage_Descent);
            //SubscribeComponent<EffectEvolutionComponent, ApplyEffectDamageEvent>(ApplyDamage_Evolution);
            //SubscribeComponent<EffectNameComponent, ApplyEffectDamageEvent>(ApplyDamage_Name);
            //SubscribeComponent<EffectSodaComponent, ApplyEffectDamageEvent>(ApplyDamage_Soda);
            //SubscribeComponent<EffectCupsuleComponent, ApplyEffectDamageEvent>(ApplyDamage_Cupsule);
        }

        private void ApplyDamage_GainAlly(EntityUid uid, EffectGainAllyComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:2892 	if (cc!pc)&(cc<maxFollower):txtNothingHappen:swbr ...
            var filter = component.CharaFilter;

            // TODO CharaFilter.Clone()
            if (_rand.OneIn(3))
                filter.Tags = new[] { Protos.Tag.CharaMan };
            else
                filter.Tags = null;

            filter.MinLevel = args.Damage;
            filter.Quality = Quality.Good;
            filter.CommonArgs.NoLevelScaling = true;
            filter.CommonArgs.NoRandomModify = true;

            var chara = _charaGen.GenerateChara(args.Source, filter);
            if (!IsAlive(chara))
            {
                return;
            }

            if (component.MessageKey != null)
                _mes.Display(Loc.GetString(component.MessageKey.Value, ("source", args.Source), ("ally", chara.Value)));

            _parties.TryRecruitAsAlly(args.Source, chara.Value);

            args.Success();
            // <<<<<<<< elona122/shade2/proc.hsp:2910 	rc=nc:gosub *add_ally ...
        }

        private bool RegenerateArtifact(EntityUid targetChara, EntityUid? materialKit, EntityUid targetItem, int damage)
        {
            if (damage < 350 || !TryProtoID(targetItem, out var protoID))
            {
                _mes.Display(Loc.GetString("Elona.Effect.ChangeMaterial.Artifact.MorePowerNeeded", ("targetChara", targetChara), ("targetItem", targetItem), ("materialKit", materialKit), ("damage", damage)));
                return false;
            }

            var anim = new BasicAnimMapDrawable(Protos.BasicAnim.AnimSmoke);
            _mapDrawables.Enqueue(anim, targetItem);

            _mes.Display(Loc.GetString("Elona.Effect.ChangeMaterial.Artifact.Reconstructed", ("targetChara", targetChara), ("targetItem", targetItem), ("materialKit", materialKit), ("damage", damage)));

            var coords = Spatial(targetItem).Coordinates;
            EntityManager.DeleteEntity(targetItem);
            _itemGen.GenerateItem(coords, protoID.Value);

            return true;
        }

        private bool DoChangeMaterial(EntityUid targetChara, EntityUid? materialKit, EntityUid targetItem, int damage)
        {
            if (_qualities.GetQuality(targetItem) == Quality.Unique)
            {
                return RegenerateArtifact(targetChara, materialKit, targetItem, damage);
            }
            else
            {
                var anim = new BasicAnimMapDrawable(Protos.BasicAnim.AnimSmoke);
                _mapDrawables.Enqueue(anim, targetItem);

                var material = Protos.Material.Fresh;
                if (IsAlive(materialKit) && TryComp<MaterialComponent>(materialKit.Value, out var matComp))
                {
                    material = matComp.MaterialID ?? material;
                }

                if (damage <= 50 && _rand.OneIn(3))
                {
                    material = Protos.Material.Fresh;
                }

                var oldItemName = _displayNames.GetDisplayName(targetItem, amount: 1);

                var quality = EnumHelpers.Clamp<Quality>((Quality)(damage / 100));

                var newMaterial = _materials.PickRandomMaterialIDRaw(damage / 10, quality, material, targetItem);

                return true; // TODO
            }
        }

        private void ApplyDamage_ChangeMaterial(EntityUid uid, EffectChangeMaterialComponent component, ApplyEffectDamageEvent args)
        {
            if (!IsAlive(args.InnerTarget) || !_gameSession.IsPlayer(args.InnerTarget.Value))
            {
                args.OutEffectWasObvious = false;
                return;
            }

            EntityUid targetItem;

            if (IsAlive(args.CommonArgs.TargetItem))
            {
                targetItem = args.CommonArgs.TargetItem.Value;
            }
            else
            {
                var context = new InventoryContext(args.InnerTarget.Value,
                     new MatchComponentsInventoryBehavior(new Type[] { typeof(EquipmentComponent), typeof(FurnitureComponent) }, matchKind: MatchComponentKind.Any, InventoryIcon.PickUp));
                var result = _uiManager.Query<InventoryLayer, InventoryContext, InventoryLayer.Result>(context);

                if (!result.HasValue || !IsAlive(result.Value.SelectedItem) || !_stacks.TrySplit(result.Value.SelectedItem.Value, 1, out targetItem))
                {
                    args.OutEffectWasObvious = false;
                    return;
                }
            }

            // XXX: Could be replaced with an event.
            if (!component.ApplyToGodlyAndLivingWeapons)
            {
                if (_qualities.GetQuality(targetItem) == Quality.God || HasComp<LivingWeaponComponent>(targetItem))
                {
                    args.OutEffectWasObvious = false;
                    return;
                }
            }

            if (DoChangeMaterial(args.InnerTarget.Value, args.CommonArgs.SourceItem, targetItem, args.Damage))
                args.Success();
            else
                args.Failure();
        }
    }
}
