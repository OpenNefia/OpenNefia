using OpenNefia.Content.Maps;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.World;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UserInterface;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Parties;

namespace OpenNefia.Content.Scenarios
{
    public sealed partial class VanillaScenariosSystem
    {
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IPartySystem _parties = default!;

        public override void Initialize()
        {
            SubscribeComponent<MapVernisComponent, MapEnterEvent>(Vernis_FirstAlly);
        }

        private void Vernis_FirstAlly(EntityUid uid, MapVernisComponent component, MapEnterEvent args)
        {
            if (_world.State.HasMetFirstAlly)
                return;

            _world.State.HasMetFirstAlly = true;
            _deferredEvents.Enqueue(() => MeetFirstAlly(canCancel: false));
        }

        private readonly PrototypeId<EntityPrototype>[] FirstAllyChoices = new[]
        {
            Protos.Chara.Dog,
            Protos.Chara.Cat,
            Protos.Chara.BrownBear,
            Protos.Chara.LittleGirl
        };

        public TurnResult MeetFirstAlly(bool canCancel = false)
        {
            var choices = FirstAllyChoices.Select((id, i) =>
            {
                return new RandomEventPrompt.Choice(i, Loc.GetString($"Elona.Scenario.FirstAlly.Event.Choices.{id}"));
            }).ToList();
            
            // From omake.
            if (canCancel)
            {
                choices.Add(new RandomEventPrompt.Choice(choices.Count, Loc.GetString("Elona.Scenario.FirstAlly.Event.Choices.Cancel")));
            }

            var promptArgs = new RandomEventPrompt.Args()
            {
                Title = Loc.GetString("Elona.Scenario.FirstAlly.Event.Title"),
                Text = Loc.GetString("Elona.Scenario.FirstAlly.Event.Text"),
                Image = Protos.Asset.BgRe13,
                Choices = choices
            };

            var result = _uiManager.Query<RandomEventPrompt, RandomEventPrompt.Args, RandomEventPrompt.Result>(promptArgs);

            if (!result.HasValue 
                || result.Value.Choice == null
                // Check if the cancel choice was selected (last choice).
                || (canCancel && result.Value.Choice.ChoiceIndex == choices.Count - 1))
                return TurnResult.Aborted;

            var protoID = FirstAllyChoices[result.Value.Choice!.ChoiceIndex];

            var filter = new CharaFilter()
            {
                Id = protoID,
                LevelOverride = _levels.GetLevel(_gameSession.Player) * 2 / 3 + 1
            };
            var ally = _charaGen.GenerateChara(_gameSession.Player, filter);
            if (IsAlive(ally))
                _parties.TryRecruitAsAlly(_gameSession.Player, ally.Value);

            return TurnResult.Aborted;
        }
    }
}
