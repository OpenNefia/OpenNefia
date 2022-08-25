using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Items;
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
using OpenNefia.Content.Qualities;
using OpenNefia.Core.Configuration;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.Levels;
using OpenNefia.Content.UI;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.UserInterface;
using PrettyPrompt;
using System.Security.Cryptography;
using OpenNefia.Content.Enchantments;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Containers;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.LivingWeapon
{
    public interface ILivingWeaponSystem : IEntitySystem
    {
        void AddLivingWeaponComponent(EntityUid item);
        int CalcLivingWeaponExperienceToNext(int level);
    }

    public sealed class LivingWeaponSystem : EntitySystem, ILivingWeaponSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IQualitySystem _qualities = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly IEnchantmentSystem _enchantments = default!;
        [Dependency] private readonly IEnchantmentGenSystem _enchantmentGen = default!;

        public override void Initialize()
        {
            SubscribeComponent<WeaponComponent, EntityBeingGeneratedEvent>(Weapon_ProcAddLivingWeapon, priority: EventPriorities.Low);
            SubscribeEntity<AfterPhysicalAttackHitEventArgs>(ProcLivingWeaponExperienceGain);
            SubscribeComponent<LivingWeaponComponent, GetItemDescriptionEventArgs>(LivingWeapon_GetItemDesc);
            SubscribeComponent<LivingWeaponComponent, GetVerbsEventArgs>(LivingWeapon_GetVerbs);
        }

        private void Weapon_ProcAddLivingWeapon(EntityUid uid, WeaponComponent component, ref EntityBeingGeneratedEvent args)
        {
            var quality = _qualities.GetQuality(uid);
            if (quality != Quality.Great && quality != Quality.God)
                return;

            // >>>>>>>> elona122/shade2/item_data.hsp:787 		if (rnd(100)=0)or(dbg_WeaponAlive) : if (refType ...
            if (_rand.OneIn(100) || _config.GetCVar(CCVars.DebugLivingWeapon))
            {
                AddLivingWeaponComponent(uid);
            }
            // <<<<<<<< elona122/shade2/item_data.hsp:791 			} ...
        }

        private void ProcLivingWeaponExperienceGain(EntityUid attacker, AfterPhysicalAttackHitEventArgs args)
        {
            // >>>>>>>> elona122/shade2/action.hsp:1376 	if attackSkill!rsMartial:if cExist(tc)!cAlive:cw= ...
            if (IsAlive(args.Weapon) && !IsAlive(args.Target) && TryComp<LivingWeaponComponent>(args.Weapon, out var livingWeapon))
            {
                if (livingWeapon.Experience < livingWeapon.ExperienceToNext)
                {
                    livingWeapon.Experience += _rand.Next(_levels.GetLevel(args.Target) / livingWeapon.Level.Base + 1);
                    if (livingWeapon.Experience >= livingWeapon.ExperienceToNext)
                    {
                        livingWeapon.Experience = livingWeapon.ExperienceToNext;
                        _audio.Play(Protos.Sound.Ding2);
                        _mes.Display(Loc.GetString("Elona.LivingWeapon.HasTastedEnoughBlood", ("item", args.Weapon.Value)), color: UiColors.MesGreen);
                    }
                }
            }
            // <<<<<<<< elona122/shade2/action.hsp:1381 	} ...
        }

        private void LivingWeapon_GetItemDesc(EntityUid uid, LivingWeaponComponent component, GetItemDescriptionEventArgs args)
        {
            args.OutEntries.Add(new ItemDescriptionEntry()
            {
                Text = Loc.GetString("Elona.LivingWeapon.ItemDescription.ItIsAlive",
                                        ("item", uid),
                                        ("level", component.Level.Base),
                                        ("levelBuffed", component.Level.Buffed),
                                        ("exp", component.Experience),
                                        ("expToNext", component.ExperienceToNext))
            });
        }

        private void LivingWeapon_GetVerbs(EntityUid uid, LivingWeaponComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(UseInventoryBehavior.VerbTypeUse, "Grow Living Weapon", () => GrowLivingWeapon(args.Source, args.Target)));
        }

        public void AddLivingWeaponComponent(EntityUid item)
        {
            if (HasComp<LivingWeaponComponent>(item))
                return;

            var livingWeapon = EnsureComp<LivingWeaponComponent>(item);
            livingWeapon.ExperienceToNext = CalcLivingWeaponExperienceToNext(livingWeapon.Level.Base);
            livingWeapon.RandomSeed = _rand.Next();

            // TODO gender component
        }

        public int CalcLivingWeaponExperienceToNext(int level)
        {
            // >>>>>>>> elona122/shade2/calculation.hsp:13 #defcfunc calcExpAlive int lv ...
            return level * 100;
            // <<<<<<<< elona122/shade2/calculation.hsp:14 	return lv*100 ...
        }

        public const int LivingWeaponEnchantmentChoices = 3;

        private TurnResult GrowLivingWeapon(EntityUid source, EntityUid weapon, LivingWeaponComponent? livingWeapon = null)
        {
            // >>>>>>>> elona122/shade2/action.hsp:1739 	if iBit(iAlive,ci){ ...
            if (!Resolve(weapon, ref livingWeapon))
                return TurnResult.Aborted;

            if (livingWeapon.Experience < livingWeapon.ExperienceToNext)
            {
                _mes.Display(Loc.GetString("Elona.LivingWeapon.Grow.NeedsMoreBlood", ("item", weapon)));
                return TurnResult.Aborted;
            }

            _mes.Display(Loc.GetString("Elona.LivingWeapon.Grow.ReadyToGrow", ("item", weapon)));

            bool powerIsBecomingAThreat = false;

            _rand.WithSeed(livingWeapon.RandomSeed, () =>
            {
                powerIsBecomingAThreat = livingWeapon.Level >= 4 + _rand.Next(12);
            });

            if (powerIsBecomingAThreat)
                _mes.Display(Loc.GetString("Elona.LivingWeapon.Grow.ButYouSensedSomethingWeird", ("item", weapon)));

            _mes.Display(Loc.GetString("Elona.LivingWeapon.Grow.It", ("item", weapon)));

            // TODO hack
            // easier way of spawning a temporary container? it requires an attached entity.
            var tempContainer = EntityManager.SpawnEntity(null, MapCoordinates.Global);
            var tempEncs = EnsureComp<EnchantmentsComponent>(tempContainer);
            
            var choices = new List<ILivingWeaponUpgrade>();

            for (var i = 0; i < LivingWeaponEnchantmentChoices; i++)
            {
                var seed = livingWeapon.RandomSeed + livingWeapon.Level.Base * 10 + i;

                _rand.WithSeed(seed, () =>
                {
                    var encLevel = _enchantmentGen.CalcRandomEnchantmentLevel();
                    var power = _enchantmentGen.CalcRandomEnchantmentPower();
                    var encID = _enchantmentGen.PickRandomEnchantmentID(weapon, encLevel);
                    if (encID != null)
                    {
                        var enc = _enchantments.SpawnEnchantment(tempEncs.Container, encID.Value, weapon, ref power, source: EnchantmentSources.LivingWeapon);
                        if (IsAlive(enc))
                        {
                            choices.Add(new LivingWeaponEnchantmentUpgrade(enc.Value, power, weapon));
                        }
                    }
                });
            }

            choices.Add(new LivingWeaponAddBonusUpgrade());

            var args = new Prompt<ILivingWeaponUpgrade>.Args(choices);

            var result = _uiManager.Query<Prompt<ILivingWeaponUpgrade>, Prompt<ILivingWeaponUpgrade>.Args, PromptChoice<ILivingWeaponUpgrade>>(args);

            if (result.HasValue)
            {
                var upgrade = result.Value.ChoiceData;
                upgrade.Apply(weapon);

                _audio.Play(Protos.Sound.Ding3);
                _mes.Display(Loc.GetString("Elona.LivingWeapon.Grow.VibratesPleased", ("item", weapon)), UiColors.MesGreen);

                if (powerIsBecomingAThreat)
                {
                    _mes.Display(Loc.GetString("Elona.LivingWeapon.Grow.ItsPowerIsBecomingAThreat", ("item", weapon)));
                    var enc = _enchantments.AddEnchantment(weapon, Protos.Enchantment.SuckBlood, 50, source: EnchantmentSources.LivingWeapon);
                    if (!IsAlive(enc))
                    {
                        var toDelete = _enchantments.EnumerateEnchantments(weapon).LastOrDefault();
                        if (toDelete != null)
                        {
                            EntityManager.DeleteEntity(toDelete.Owner);

                            _mes.Display(Loc.GetString("Elona.LivingWeapon.Grow.RemovesAnEnchantment", ("item", weapon)));
                        }
                    }
                }

                livingWeapon.Level.Base++;
                livingWeapon.Experience = 0;
                livingWeapon.ExperienceToNext = CalcLivingWeaponExperienceToNext(livingWeapon.Level.Base);
            }
            else
            {
                _mes.Display(Loc.GetString("Elona.LivingWeapon.Grow.VibratesDispleased", ("item", weapon)));
            }

            EntityManager.DeleteEntity(tempContainer);
            _refresh.Refresh(source);

            return TurnResult.Aborted;
            // <<<<<<<< elona122/shade2/action.hsp:1780 		} ...
        }
    }

    public interface ILivingWeaponUpgrade : IPromptFormattable
    {
        void Apply(EntityUid livingWeapon);
    }

    public sealed class LivingWeaponAddBonusUpgrade : ILivingWeaponUpgrade
    {
        [Dependency] private readonly IEntityManager _entities = default!;

        public LivingWeaponAddBonusUpgrade()
        {
            EntitySystem.InjectDependencies(this);
        }

        public string FormatForPrompt() => Loc.GetString("Elona.LivingWeapon.Grow.Choices.AddBonus");

        public void Apply(EntityUid livingWeapon)
        {
            _entities.EnsureComponent<BonusComponent>(livingWeapon).Bonus++;
        }
    }

    public sealed class LivingWeaponEnchantmentUpgrade : ILivingWeaponUpgrade
    {
        [Dependency] private readonly IEntityManager _entities = default!;
        [Dependency] private readonly IEnchantmentSystem _enchantments = default!;

        public EntityUid EnchantmentUID { get; }
        public int EnchantmentPower { get; }
        public EntityUid Weapon { get; }

        public LivingWeaponEnchantmentUpgrade(EntityUid encUID, int power, EntityUid weapon)
        {
            EntitySystem.InjectDependencies(this);

            EnchantmentUID = encUID;
            EnchantmentPower = power;
            Weapon = weapon;
        }

        public string FormatForPrompt()
        {
            return _enchantments.GetEnchantmentDescription(EnchantmentUID, Weapon, noPowerText: true);
        }

        public void Apply(EntityUid livingWeapon)
        {
            var encPower = _entities.GetComponent<EnchantmentComponent>(EnchantmentUID).TotalPower;
            
            _enchantments.AddEnchantment(livingWeapon, EnchantmentUID, encPower, source: EnchantmentSources.LivingWeapon);
        }
    }
}