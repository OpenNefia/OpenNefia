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
        int GetFlag(PrototypeId<SidequestPrototype> sidequestID);
        void SetFlag(PrototypeId<SidequestPrototype> sidequestID, int flag);
    }

    public sealed class SidequestSystem : EntitySystem, ISidequestSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        [DataDefinition]
        private sealed class SidequestInstance
        {
            [DataField(required: true)]
            public int Flag { get; set; } = 0;
        }

        [RegisterSaveData("Elona.SidequestSystem.SidequestData")]
        private Dictionary<PrototypeId<SidequestPrototype>, SidequestInstance> _sidequestData { get; set; } = new();

        public override void Initialize()
        {
        }

        public int GetFlag(PrototypeId<SidequestPrototype> sidequestID)
        {
            var instance = _sidequestData.GetOrInsertNew(sidequestID);
            return instance.Flag;
        }

        public void SetFlag(PrototypeId<SidequestPrototype> sidequestID, int flag)
        {
            var instance = _sidequestData.GetOrInsertNew(sidequestID);
            instance.Flag = flag;
        }
    }
}