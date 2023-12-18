using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
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
using OpenNefia.Content.Factions;
using OpenNefia.Core.Game;
using OpenNefia.Content.TurnOrder;

namespace OpenNefia.Content.Effects.New.EffectTypes
{
    public sealed class VanillaEffectTypesSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        public override void Initialize()
        {
        }
    }
}