using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;

namespace OpenNefia.Content.Quests
{
    public interface IQuestSystem : IEntitySystem
    {
        IReadOnlyList<IQuest> Quests { get; }

        void UpdateInMap(IMap map);
    }

    [ImplicitDataDefinitionForInheritors]
    public interface IQuest
    {
        EntityUid QuestEntityUid { get; }
    }

    public sealed class Quest : IQuest
    {
        [DataField(required: true)]
        public EntityUid QuestEntityUid { get; internal set; }
    }

    public sealed class QuestSystem : EntitySystem, IQuestSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;

        [RegisterSaveData("Elona.QuestSystem.Quests")]
        private List<IQuest> _quests { get; } = new();
        public IReadOnlyList<IQuest> Quests => _quests;

        public void UpdateInMap(IMap map)
        {
            // TODO
        }

        public bool TryCreateQuest(PrototypeId<EntityPrototype> questID, EntityUid client, out QuestComponent? quest)
        {
            var questProto = _protos.Index(questID);
            if (!questProto.Components.HasComponent<QuestComponent>())
            {
                Logger.ErrorS("quest", $"Entity prototype {questID} cannot be used as a quest since it is missing a {nameof(QuestComponent)}!");
                quest = null;
                return false;
            }

            if (!TryMap(client, out var map))
            {
                quest = null;
                return false;
            }

            quest = null;
            return false;
        }
    }
}
