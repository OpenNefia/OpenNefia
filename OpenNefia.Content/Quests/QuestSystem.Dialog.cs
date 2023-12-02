using OpenNefia.Content.Dialog;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Quests
{
    public sealed partial class QuestSystem
    {
        public QualifiedDialogNode? QuestClient_About(IDialogEngine engine, IDialogNode node)
        {
            var quest = engine.Data.Get<DialogQuestData>();

            var localized = LocalizeQuestData(quest.Quest, engine.Speaker!.Value, engine.Player);

            var texts = new List<DialogTextEntry>()
            {
                DialogTextEntry.FromString(localized.Description)
            };

            var choices = new List<DialogChoiceEntry>()
            {
                new DialogChoiceEntry()
                {
                    NextNode = new(Protos.Dialog.QuestClient, "BeforeAccept"),
                    Text = DialogTextEntry.FromLocaleKey("Elona.Quest.Dialog.About.Choices.Take")
                },
                new DialogChoiceEntry()
                {
                    NextNode = new(Protos.Dialog.Default, "YouKidding"),
                    Text = DialogTextEntry.FromLocaleKey("Elona.Quest.Dialog.About.Choices.Leave"),
                    IsDefault = true
                }
            };

            var nextNode = new DialogTextNode(texts, choices);

            return new QualifiedDialogNode(Protos.Dialog.QuestClient, nextNode);
        }

        public const int MaxAcceptedQuests = 5;

        public QualifiedDialogNode? QuestClient_BeforeAccept(IDialogEngine engine, IDialogNode node)
        {
            var quest = engine.Data.Get<DialogQuestData>();
            var questComp = Comp<QuestComponent>(quest.Quest);

            if (EnumerateAcceptedQuests().Count() >= MaxAcceptedQuests)
            {
                return engine.GetNodeByID(Protos.Dialog.QuestClient, "TooManyUnfinished");
            }

            var nextNodeID = new QualifiedDialogNodeID(Protos.Dialog.QuestClient, "Accept");
            var ev = new QuestBeforeAcceptEvent(questComp, nextNodeID);
            RaiseEvent(quest.Quest, ev);

            if (!ev.Cancelled)
            {
                questComp.State = QuestState.Accepted;
                if (questComp.TimeAllotted != null)
                    questComp.Deadline = _world.State.GameDate + questComp.TimeAllotted.Value;
            }

            return engine.GetNodeByID(ev.OutNextDialogNodeID);
        }
    }

    public sealed class DialogQuestData : IDialogExtraData
    {
        public DialogQuestData() { }

        public DialogQuestData(EntityUid quest)
        {
            Quest = quest;
        }

        [DataField]
        public EntityUid Quest { get; }
    }
}
