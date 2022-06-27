using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Quests
{
    public interface IQuestSystem : IEntitySystem
    {
        void UpdateInMap(IMap map);
    }

    public sealed class QuestSystem : EntitySystem, IQuestSystem
    {
        public void UpdateInMap(IMap map)
        {
            // TODO
        }
    }
}
