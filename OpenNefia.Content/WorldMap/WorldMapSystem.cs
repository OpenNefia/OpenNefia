using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.WorldMap
{
    public interface IWorldMapSystem : IEntitySystem
    {
    }

    public sealed class WorldMapSystem : EntitySystem, IWorldMapSystem
    {
        [Dependency] private readonly IMapRenderer _mapRenderer = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<MapEnterEvent>(CheckCloudLayer);
        }

        private void CheckCloudLayer(MapEnterEvent ev)
        {
            var isWorldMap = HasComp<MapTypeWorldMapComponent>(ev.Map.MapEntityUid);
            _mapRenderer.SetTileLayerEnabled<CloudTileLayer>(isWorldMap);
        }
    }
}