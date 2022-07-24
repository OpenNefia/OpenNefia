using OpenNefia.Content.Hunger;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Sleep;
using OpenNefia.Content.World;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.ExHelp
{
    public interface IExHelpSystem : IEntitySystem
    {
        void Show(PrototypeId<ExHelpPrototype> id);
    }

    public sealed class ExHelpSystem : EntitySystem, IExHelpSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        public override void Initialize()
        {
            SubscribeBroadcast<MapOnTimePassedEvent>(ExHelpOnTimePassed, priority: EventPriorities.Lowest);
        }

        private void ExHelpOnTimePassed(ref MapOnTimePassedEvent ev)
        {
            if (_world.State.AwakeTime >= SleepSystem.SleepThresholdLight)
            {
                Show(Protos.ExHelp.Sleep);
            }

            if (TryComp<HungerComponent>(_gameSession.Player, out var hunger)
                && hunger.Nutrition < HungerLevels.Normal)
            {
                Show(Protos.ExHelp.Hunger);
            }
        }

        public void Show(PrototypeId<ExHelpPrototype> id)
        {
            // TODO
        }
    }
}