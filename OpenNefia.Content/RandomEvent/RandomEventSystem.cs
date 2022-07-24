using NetVips;
using OpenNefia.Content.Activity;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Religion;
using OpenNefia.Content.Skills;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.World;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.RandomEvent
{
    public interface IRandomEventChooser
    {
        PrototypeId<RandomEventPrototype>? PickRandomEventID();
    }

    public abstract class BaseRandomEventChooser : IRandomEventChooser
    {
        [Dependency] protected readonly IGameSessionManager GameSession = default!;
        [Dependency] protected readonly IEntityManager EntityManager = default!;
        [Dependency] protected readonly IMapManager MapManager = default!;
        [Dependency] protected readonly IRandom Rand = default!;

        public abstract PrototypeId<RandomEventPrototype>? PickRandomEventID();
    }

    public sealed class NullRandomEventChooser : BaseRandomEventChooser
    {
        public override PrototypeId<RandomEventPrototype>? PickRandomEventID() => null;
    }

    public sealed class DefaultRandomEventChooser : BaseRandomEventChooser
    {
        [Dependency] private readonly IActivitySystem _activities = default!;

        public override PrototypeId<RandomEventPrototype>? PickRandomEventID()
        {
            PrototypeId<RandomEventPrototype>? id = null;
            var player = GameSession.Player;

            if (!MapManager.TryGetMapOfEntity(player, out var map))
                return null;

            if (!EntityManager.HasComponent<MapTypeWorldMapComponent>(map.MapEntityUid) && _activities.HasAnyActivity(player))
                return null;

            if (EntityManager.HasComponent<MapTypePlayerOwnedComponent>(map.MapEntityUid))
                return null;

            if (Rand.OneIn(30))
            {
                id = Protos.RandomEvent.WanderingPriest;
            }
            if (Rand.OneIn(25))
            {
                id = Protos.RandomEvent.MadMillionaire;
            }
            if (Rand.OneIn(25))
            {
                id = Protos.RandomEvent.SmallLuck;
            }
            if (Rand.OneIn(50))
            {
                id = Protos.RandomEvent.GreatLuck;
            }
            if (Rand.OneIn(80))
            {
                id = Protos.RandomEvent.StrangeFeast;
            }
            if (Rand.OneIn(50))
            {
                id = Protos.RandomEvent.MaliciousHand;
            }
            if (Rand.OneIn(80))
            {
                id = Protos.RandomEvent.SmellOfFood;
            }

            if (EntityManager.HasComponent<MapTypeTownComponent>(map.MapEntityUid))
            {
                if (Rand.OneIn(25))
                {
                    id = Protos.RandomEvent.Murderer;
                }

                return id;
            }

            if (EntityManager.HasComponent<MapTypeWorldMapComponent>(map.MapEntityUid))
            {
                if (!Rand.OneIn(40))
                    return null;

                return id;
            }

            if (Rand.OneIn(25))
            {
                id = Protos.RandomEvent.CampingSite;
            }
            if (Rand.OneIn(25))
            {
                id = Protos.RandomEvent.Corpse;
            }

            return id;
        }
    }

    public sealed class SleepRandomEventChooser : BaseRandomEventChooser
    {
        [Dependency] private readonly IInventorySystem _inv = default!;

        public override PrototypeId<RandomEventPrototype>? PickRandomEventID()
        {
            PrototypeId<RandomEventPrototype>? id = null;
            var player = GameSession.Player;

            if (EntityManager.TryGetComponent<ReligionComponent>(player, out var religion) && religion.GodID != null
                && Rand.OneIn(12))
            {
                id = Protos.RandomEvent.GainingFaith;
            }
            if (Rand.OneIn(80))
            {
                id = Protos.RandomEvent.CreepyDream;
            }
            if (Rand.OneIn(20))
            {
                id = Protos.RandomEvent.Development;
            }
            if (Rand.OneIn(25))
            {
                id = Protos.RandomEvent.WizardsDream;
            }
            if (Rand.OneIn(100))
            {
                id = Protos.RandomEvent.CursedWhispering;
            }
            if (Rand.OneIn(20))
            {
                id = Protos.RandomEvent.Regeneration;
            }
            if (Rand.OneIn(20))
            {
                id = Protos.RandomEvent.Meditation;
            }
            if (Rand.OneIn(250) && !_inv.IsInventoryFull(player))
            {
                id = Protos.RandomEvent.TreasureOfDream;
            }
            if (Rand.OneIn(10000) && !_inv.IsInventoryFull(player))
            {
                id = Protos.RandomEvent.QuirkOfFate;
            }
            if (Rand.OneIn(70))
            {
                id = Protos.RandomEvent.MaliciousHand;
            }
            if (Rand.OneIn(200))
            {
                id = Protos.RandomEvent.LuckyDay;
            }
            if (Rand.OneIn(50))
            {
                id = Protos.RandomEvent.DreamHarvest;
            }
            if (Rand.OneIn(300))
            {
                id = Protos.RandomEvent.YourPotential;
            }
            if (Rand.OneIn(90))
            {
                id = Protos.RandomEvent.MonsterDream;
            }

            return id;
        }
    }

    public interface IRandomEventSystem : IEntitySystem
    {
        public IRandomEventChooser DefaultChooser { get; set; }

        void WithRandomEventChooser(IRandomEventChooser chooser, Action action);
        PrototypeId<RandomEventPrototype>? PickRandomEventID();
        RandomEventResult? TriggerRandomly();
        RandomEventResult Trigger(PrototypeId<RandomEventPrototype> id);
    }

    public sealed record RandomEventResult(PrototypeId<RandomEventPrototype> RandomEventId, RandomEventPrompt.Choice? Choice);

    public sealed class RandomEventSystem : EntitySystem, IRandomEventSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IHudLayer _hud = default!;
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

        private int _stackLevel = 0;
        private IRandomEventChooser _chooser = default!;

        private IRandomEventChooser _defaultChooser = new DefaultRandomEventChooser();
        public IRandomEventChooser DefaultChooser
        {
            get => _defaultChooser;
            set
            {
                _defaultChooser = value;
                EntitySystem.InjectDependencies(_defaultChooser);
                if (_stackLevel == 0)
                    _chooser = _defaultChooser;
            }
        }

        public override void Initialize()
        {
            EntitySystem.InjectDependencies(_defaultChooser);
            _chooser = _defaultChooser;

            SubscribeEntity<MapOnTimePassedEvent>(HandleTimePassed);
        }

        private void HandleTimePassed(EntityUid uid, ref MapOnTimePassedEvent args)
        {
            if (args.HoursPassed > 0)
                TriggerRandomly();
        }

        public PrototypeId<RandomEventPrototype>? PickRandomEventID()
        {
            return _chooser.PickRandomEventID();
        }

        public RandomEventResult? TriggerRandomly()
        {
            var target = _gameSession.Player;

            if (!TryComp<TurnOrderComponent>(target, out var turnOrder) || turnOrder.CurrentSpeed < 10)
                return null;
            
            if (_config.GetCVar(CCVars.DebugSkipRandomEvents))
                return null;
            
            var id = PickRandomEventID();
            if (id == null)
                return null;

            var proto = _protos.Index(id.Value);
            if (proto.LuckThresholdToSkip != null)
            {
                if (_rand.Next(_skills.Level(target, Protos.Skill.AttrLuck)) > proto.LuckThresholdToSkip.Value)
                {
                    id = Protos.RandomEvent.AvoidingMisfortune;
                }
            }

            return Trigger(id.Value);
        }

        public RandomEventResult Trigger(PrototypeId<RandomEventPrototype> id)
        {
            var target = _gameSession.Player;
            var proto = _protos.Index(id);

            var choiceCount = Math.Max(proto.ChoiceCount, 1);

            var evTriggered = new P_RandomEventOnTriggeredEvent(target);
            _protos.EventBus.RaiseEvent(proto, evTriggered);

            _hud.RefreshWidgets();

            var choices = Enumerable.Range(0, choiceCount)
                .Select(i => new RandomEventPrompt.Choice(i, Loc.GetPrototypeString(id, $"Choices.{i}")));

            var args = new RandomEventPrompt.Args()
            {
                Title = Loc.GetPrototypeString(id, "Title"),
                Text = Loc.GetPrototypeString(id, "Text"),
                Image = proto.Image,
                Choices = choices
            };
            var result = _uiManager.Query<RandomEventPrompt, RandomEventPrompt.Args, RandomEventPrompt.Result>(args);
            
            _field.RefreshScreen();

            if (result.HasValue && result.Value.Choice != null)
            {
                var evChoiceSelected = new P_RandomEventOnChoiceSelectedEvent(target, result.Value.Choice.ChoiceIndex);
                _protos.EventBus.RaiseEvent(proto, evChoiceSelected);

                return new RandomEventResult(id, result.Value.Choice);
            }

            return new RandomEventResult(id, null);
        }

        public void WithRandomEventChooser(IRandomEventChooser chooser, Action action)
        {
            var prev = _chooser;

            EntitySystem.InjectDependencies(chooser);
            _chooser = chooser;
            _stackLevel++;

            try
            {
                action();
            }
            catch (Exception ex)
            {
                Logger.Error("randomEvent", ex, "Error inside random event chooser override");
                throw ex;
            }
            finally
            {
                _chooser = prev;
                _stackLevel--;

                if (_stackLevel <= 0)
                {
                    _chooser = _defaultChooser;
                }
            }
        }
    }

    [PrototypeEvent(typeof(RandomEventPrototype))]
    public sealed class P_RandomEventOnTriggeredEvent : PrototypeEventArgs
    {
        public EntityUid Target { get; }

        public P_RandomEventOnTriggeredEvent(EntityUid target)
        {
            Target = target;
        }
    }

    [PrototypeEvent(typeof(RandomEventPrototype))]
    public sealed class P_RandomEventOnChoiceSelectedEvent : PrototypeEventArgs
    {
        public EntityUid Target { get; }
        public int ChoiceIndex { get; }

        public P_RandomEventOnChoiceSelectedEvent(EntityUid target, int choiceIndex)
        {
            Target = target;
            ChoiceIndex = choiceIndex;
        }
    }
}
