using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.TurnOrder;
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
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Game;
using OpenNefia.Content.PCCs;

namespace OpenNefia.Content.Rendering
{
    public sealed class PlayerLightSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IEntityDrawablesSystem _entityDrawables = default!;

        public const string DrawableID = "Elona.PlayerLight";

        public override void Initialize()
        {
            SubscribeEntity<PlayerTurnStartedEvent>(InitPlayerLight);
        }

        private void InitPlayerLight(EntityUid uid, ref PlayerTurnStartedEvent ev)
        {
            if (!_entityDrawables.HasDrawable(uid, DrawableID))
            {
                var entityDrawable = new EntityDrawableEntry(new PlayerLightDrawable(), zOrder: -10000);
                _entityDrawables.RegisterDrawable(uid, DrawableID, entityDrawable);
            }
        }
    }
}