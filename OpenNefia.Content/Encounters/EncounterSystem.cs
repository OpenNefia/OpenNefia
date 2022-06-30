using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Encounters
{
    public interface IEncounterSystem : IEntitySystem
    {
        string? PickRandomEncounterID();
        void StartEncounter(string id);
    }

    public sealed class EncounterSystem : EntitySystem, IEncounterSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        public override void Initialize()
        {
        }

        public string? PickRandomEncounterID()
        {
            // TODO
            return null;
        }

        public void StartEncounter(string id)
        {
            if (_config.GetCVar(CCVars.DebugNoEncounters))
                return;

            // TODO
        }
    }
}