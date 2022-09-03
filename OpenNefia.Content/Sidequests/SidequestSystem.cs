using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Sidequests
{
    public interface ISidequestSystem : IEntitySystem
    {
        int GetState(PrototypeId<SidequestPrototype> sidequestID);
        void SetState(PrototypeId<SidequestPrototype> sidequestID, int flag);
    }

    public sealed class SidequestSystem : EntitySystem, ISidequestSystem
    {
        [DataDefinition]
        private sealed class SidequestInstance
        {
            [DataField(required: true)]
            public int State { get; set; } = 0;
        }

        [RegisterSaveData("Elona.SidequestSystem.SidequestData")]
        private Dictionary<PrototypeId<SidequestPrototype>, SidequestInstance> _sidequestData { get; set; } = new();

        public override void Initialize()
        {
        }

        public int GetState(PrototypeId<SidequestPrototype> sidequestID)
        {
            var instance = _sidequestData.GetOrInsertNew(sidequestID);
            return instance.State;
        }

        public void SetState(PrototypeId<SidequestPrototype> sidequestID, int state)
        {
            var instance = _sidequestData.GetOrInsertNew(sidequestID);
            instance.State = state;
        }
    }
}