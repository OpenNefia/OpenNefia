using OpenNefia.Content.Logic;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.World;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Rendering.TileDrawLayers;

namespace OpenNefia.Content.Maps
{
    public interface IMapShadowSystem : IEntitySystem
    {
        Color CalcMapShadow(IMap map);
    }

    public sealed class MapShadowSystem : EntitySystem, IMapShadowSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IMapRenderer _mapRenderer = default!;
        [Dependency] private readonly IFieldLayer _field = default!;

        public static readonly Color DefaultShadow = new Color(5, 5, 5);

        public override void Initialize()
        {
            _field.OnScreenRefresh += () =>
            {
                if (_mapManager.ActiveMap != null)
                    UpdateMapShadow(_mapManager.ActiveMap);
            };

            SubscribeComponent<MapCommonComponent, MapOnTimePassedEvent>(UpdateMapShadow);
        }

        private void UpdateMapShadow(EntityUid uid, MapCommonComponent component, ref MapOnTimePassedEvent args)
        {
            UpdateMapShadow(args.Map);
        }

        private void UpdateMapShadow(IMap map)
        {
            var layer = _mapRenderer.GetTileLayer<TileAndChipTileLayer>();
            layer.TileShadow = CalcMapShadow(map);
        }

        public Color CalcMapShadow(IMap map)
        {
            if (!TryComp<MapCommonComponent>(map.MapEntityUid, out var common) || common.IsIndoors)
                return DefaultShadow;

            var shadow = DefaultShadow;

            var hour = _world.State.GameDate.Hour;

            if (hour >= 24 || (hour >= 0 && hour < 4))
            {
                shadow = new Color(110, 90, 60);
            }
            else if (hour >= 4 && hour < 10)
            {
                shadow = new Color(70 - (hour - 3) * 10, 80 - (hour - 3) * 12, 60 - (hour - 3) * 10);
            }
            else if (hour >= 10 && hour < 12)
            {
                shadow = new Color(10, 10, 10);
            }
            else if (hour >= 12 && hour < 17)
            {
                shadow = Color.Black;
            }
            else if (hour >= 17 && hour < 21)
            {
                shadow = new Color(0 + (hour - 17) * 20, 15 + (hour - 16) * 15, 10 + (hour - 16) * 10);
            }
            else if (hour >= 21 && hour < 24)
            {
                shadow = new Color(80 + (hour - 21) * 10, 70 + (hour - 21) * 10, 40 + (hour - 21) * 5);
            }

            var ev = new CalcMapShadowEvent(shadow);
            RaiseEvent(map.MapEntityUid, ref ev);
            return ev.OutShadow;
        }
    }

    [ByRefEvent]
    public struct CalcMapShadowEvent
    {
        public Color OutShadow { get; set; }

        public CalcMapShadowEvent(Color shadow)
        {
            OutShadow = shadow;
        }
    }
}