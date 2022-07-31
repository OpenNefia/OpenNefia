using OpenNefia.Content.Damage;
using OpenNefia.Content.Effects;
using OpenNefia.Content.Feats;
using OpenNefia.Content.GameController;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.Hud;
using OpenNefia.Content.Hunger;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomEvent;
using OpenNefia.Content.Religion;
using OpenNefia.Content.SaveLoad;
using OpenNefia.Content.Skills;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.UI;
using OpenNefia.Content.World;
using OpenNefia.Core;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.GameController;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Sleep
{
    public interface ISleepSystem : IEntitySystem
    {
        bool IsPlayerSleeping { get; }

        void Sleep(EntityUid sleeper, EntityUid? bed = null, GameTimeSpan? timeSlept = null);
    }

    public sealed class SleepSystem : EntitySystem, ISleepSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IRandomEventSystem _randomEvents = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IReligionSystem _religion = default!;
        [Dependency] private readonly IMusicManager _music = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;
        [Dependency] private readonly IHungerSystem _hunger = default!;
        [Dependency] private readonly CommonEffectsSystem _commonEffects = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly ITagSystem _tags = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly ISaveLoadSystem _saveLoad = default!;
        [Dependency] private readonly MapCommonSystem _mapCommon = default!;
        [Dependency] private readonly IGameController _gameController = default!;

        public override void Initialize()
        {
            SubscribeEntity<GetStatusIndicatorsEvent>(AddStatusIndicator);
        }

        public static readonly GameTimeSpan SleepThresholdLight = GameTimeSpan.FromHours(15);
        public static readonly GameTimeSpan SleepThresholdModerate = GameTimeSpan.FromHours(30);
        public static readonly GameTimeSpan SleepThresholdHeavy = GameTimeSpan.FromHours(50);

        private void AddStatusIndicator(EntityUid uid, GetStatusIndicatorsEvent args)
        {
            Color? color = null;
            LocaleKey? key = null;
            var awakeTime = _world.State.AwakeTime;

            if (awakeTime >= SleepThresholdHeavy)
            {
                color = UiColors.SleepIndicatorHeavy;
                key = "Elona.Sleep.Indicator.Heavy";
            }
            else if (awakeTime >= SleepThresholdModerate)
            {
                color = UiColors.SleepIndicatorModerate;
                key = "Elona.Sleep.Indicator.Moderate";
            }
            else if (awakeTime >= SleepThresholdLight)
            {
                color = UiColors.SleepIndicatorLight;
                key = "Elona.Sleep.Indicator.Light";
            }

            if (color != null && key != null)
                args.OutIndicators.Add(new() { Text = Loc.GetString(key.Value), Color = color.Value });
        }
        public bool IsPlayerSleeping { get; private set; } = false;

        private bool CanSleepRightNow(EntityUid sleeper)
        {
            // TODO immediate quests
            return TryMap(sleeper, out var map) && !HasComp<MapTypeQuestComponent>(map.MapEntityUid);
        }

        private int IncrementSleepPotential(EntityUid sleeper, EntityUid? bed)
        {
            if (!TryComp<SleepExperienceComponent>(sleeper, out var sleepExp))
                return 0;

            var bedQuality = 0f;
            if (IsAlive(bed) && TryComp<BedComponent>(bed, out var bedComp))
                bedQuality = bedComp.BedQuality;

            var totalAttrLevels = _skills.EnumerateBaseAttributes()
                .Aggregate(0, (total, attr) => total + _skills.BaseLevel(sleeper, attr));

            totalAttrLevels = Math.Clamp(totalAttrLevels / 6, 10, 1000);
            var sleepExpPerGrowth = totalAttrLevels * totalAttrLevels * totalAttrLevels / 10;
            sleepExp.SleepExperience = (int)(sleepExp.SleepExperience * bedQuality);

            var grownCount = 0;

            while (true)
            {
                if (sleepExp.SleepExperience >= sleepExpPerGrowth)
                {
                    sleepExp.SleepExperience -= sleepExpPerGrowth;
                }
                else if (grownCount != 0) // Always grow at least one attribute.
                {
                    break;
                }

                var attr = _skills.PickRandomBaseAttribute();
                _skills.ModifyPotential(sleeper, attr, 1);
                grownCount++;

                if (grownCount > 6)
                {
                    if (_rand.OneIn(5))
                    {
                        sleepExp.SleepExperience = 0;
                        break;
                    }
                }
            }

            return grownCount;
        }

        private void ApplySleepEffects(EntityUid entity)
        {
            var ev = new OnCharaSleepEvent();
            RaiseEvent(entity, ev);

            _damage.HealToMax(entity);
        }

        private const int SleepNutritionDecrementAmount = 1500;

        private void DoSleep(EntityUid sleeper, EntityUid? bed, bool noAnimation, GameTimeSpan timeSlept)
        {
            if (CompOrNull<CommonProtectionsComponent>(sleeper)?.CanCatchGodSignals == true)
            {
                _religion.GodSays(sleeper, "Elona.Sleep");
            }

            if (!noAnimation)
            {
                _music.Play(Protos.Music.Coda);
                // TODO mes halt?
            }

            // TODO animation

            _world.State.AwakeTime = GameTimeSpan.Zero;

            foreach (var partyMember in _parties.EnumerateMembers(sleeper))
            {
                ApplySleepEffects(partyMember);
            }

            for (var i = 0; i < timeSlept.TotalHours; i++)
            {
                _world.PassTime(GameTimeSpan.FromHours(1));
                var date = _world.State.GameDate;
                _world.State.GameDate = new GameDateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
                if (!noAnimation)
                {
                    _gameController.Wait(0.5f);
                }
            }

            if (TryMap(sleeper, out var map))
                _commonEffects.WakeUpEveryone(map);

            if (TryComp<HungerComponent>(sleeper, out var hunger))
            {
                var hungerAdjust = 1;
                if (_feats.HasFeat(sleeper, Protos.Feat.PermSlowFood))
                    hungerAdjust = 2;
                hunger.Nutrition -= SleepNutritionDecrementAmount / hungerAdjust;
            }

            var addPotential = IsAlive(bed) && _tags.HasTag(bed.Value, Protos.Tag.ItemCatFurnitureBed);

            if (addPotential)
            {
                var grownCount = IncrementSleepPotential(sleeper, bed);
                _mes.Display(Loc.GetString("Elona.Sleep.WakeUp.Good", ("grownCount", grownCount)), UiColors.MesGreen);
            }
            else
            {
                _mes.Display(Loc.GetString("Elona.Sleep.WakeUp.SoSo"));
            }

            if (!noAnimation)
            {
                _playerQuery.PromptMore();
                if (TryMap(sleeper, out map))
                    _mapCommon.PlayMapDefaultMusic(map);
                // TODO stop animation
            }

            _saveLoad.QueueAutosave();
        }

        private GameTimeSpan CalcTimeSlept(EntityUid sleeper)
        {
            return GameTimeSpan.FromHours(7 + _rand.Next(5));
        }

        public void Sleep(EntityUid sleeper, EntityUid? bed = null, GameTimeSpan? timeSlept = null)
        {
            if (IsPlayerSleeping)
                return;
            
            if (!CanSleepRightNow(sleeper))
            {
                _mes.Display(Loc.GetString("Elona.Sleep.ButYouCannot"));
                return;
            }

            var noAnimation = _config.GetCVar(CCVars.AnimeSkipSleepAnimation);
            if (timeSlept == null)
                timeSlept = CalcTimeSlept(sleeper);

            try
            {
                IsPlayerSleeping = true;
                _randomEvents.WithRandomEventChooser(new SleepRandomEventChooser(), () => DoSleep(sleeper, bed, noAnimation, timeSlept.Value));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                IsPlayerSleeping = false;
                var ev = new AfterSleepFinish(bed, timeSlept.Value);
                RaiseEvent(sleeper, ev);
            }
        }
    }

    public sealed class OnCharaSleepEvent : EntityEventArgs
    {
        public OnCharaSleepEvent()
        {
        }
    }

    public sealed class AfterSleepFinish : EntityEventArgs
    {
        public EntityUid? Bed { get; }
        public GameTimeSpan TimeSlept { get; }

        public AfterSleepFinish(EntityUid? bed, GameTimeSpan timeSlept)
        {
            Bed = bed;
            TimeSlept = timeSlept;
        }
    }
}