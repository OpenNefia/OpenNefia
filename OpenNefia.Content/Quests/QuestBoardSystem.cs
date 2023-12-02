using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.UserInterface;
using OpenNefia.Content.Dialog;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Spells;
using OpenNefia.Core.Game;
using OpenNefia.Core.EngineVariables;
using OpenNefia.Core.Log;

namespace OpenNefia.Content.Quests
{
    public sealed class QuestBoardSystem : EntitySystem
    {
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IQuestSystem _quests = default!;
        [Dependency] private readonly IDialogSystem _dialogs = default!;
        [Dependency] private readonly ISpellSystem _spells = default!;

        public override void Initialize()
        {
            SubscribeComponent<QuestBoardComponent, WasCollidedWithEventArgs>(QuestBoard_HandleCollidedWith);
        }

        [EngineVariable("Elona.DebugRegenQuestsEveryTime")]
        public bool DebugRegenQuestsEveryTime { get; set; } = false;

        private void QuestBoard_HandleCollidedWith(EntityUid uid, QuestBoardComponent component, WasCollidedWithEventArgs args)
        {
            if (!TryMap(uid, out var map))
                return;

            if (DebugRegenQuestsEveryTime)
            {
                Logger.WarningS("questBoard", $"DEBUG: Regenerating quests on {map.Id}");
                foreach (var quest in _quests.EnumerateAllQuests().Where(q => q.State == QuestState.NotAccepted).ToList())
                    _quests.DeleteQuest(quest);
                _quests.UpdateInMap(map);
            }

            var questsHere = _quests.EnumerateAllQuests()
                .Where(q => q.ClientOriginatingMapID == map.Id && q.State == QuestState.NotAccepted)
                .OrderBy(q => q.Difficulty)
                .ToList();

            var layerArgs = new QuestBoardLayer.Args(questsHere);
            var result = _uiManager.Query<QuestBoardLayer, QuestBoardLayer.Args, QuestBoardLayer.Result>(layerArgs);

            if (!result.HasValue)
            {
                args.Handle(TurnResult.Aborted);
                return;
            }

            var turnResult = VisitQuestGiver(args.Source, result.Value.SelectedQuest);
            args.Handle(turnResult);
        }

        private TurnResult VisitQuestGiver(EntityUid source, QuestComponent selectedQuest)
        {
            if (!TryMap(source, out var map))
                return TurnResult.Failed;

            if (!IsAlive(selectedQuest.ClientEntity))
                return TurnResult.Failed;

            var dialogNodeID = new QualifiedDialogNodeID(Protos.Dialog.QuestClient, "About");
            var extraData = new Blackboard<IDialogExtraData>();
            extraData.Add(new DialogQuestData(selectedQuest));

            _spells.Cast(Protos.Spell.ActionShadowStep, target: selectedQuest.ClientEntity, source: source);
            return _dialogs.TryToChatWith(source, selectedQuest.ClientEntity, dialogNodeID, extraData: extraData);
        }
    }
}