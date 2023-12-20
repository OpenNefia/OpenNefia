using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Resists
{
    public interface IElementSystem : IEntitySystem
    {
        void DamageTile(MapCoordinates coords, PrototypeId<ElementPrototype> elementID, EntityUid? source = null);
    }

    public sealed class ElementSystem : EntitySystem, IElementSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public override void Initialize()
        {
        }

        public void DamageTile(MapCoordinates coords, PrototypeId<ElementPrototype> elementID, EntityUid? source = null)
        {
            var ev = new P_ElementDamageTileEvent(coords, source);
            _protos.EventBus.RaiseEvent(elementID, ref ev);
        }
    }
}