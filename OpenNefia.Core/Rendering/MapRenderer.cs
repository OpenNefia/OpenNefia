using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Rendering.TileDrawLayers;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Utility;
using System.Reflection;

namespace OpenNefia.Core.Rendering
{
    public sealed class MapRenderer : BaseDrawable, IMapRenderer
    {
        private const string Sawmill = "maprenderer.sys";

        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;
        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly ITileAtlasManager _tileAtlasManager = default!;
        [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

        private DependencyCollection _layerDependencyCollection = default!;

        private sealed record OrderingData(Type OrderType, Type[]? Before, Type[]? After);

        private sealed class TileLayerMetaData
        {
            public bool Enabled { get; set; }

            public TileLayerMetaData(bool enabled)
            {
                Enabled = enabled;
            }
        }

        private readonly Dictionary<Type, OrderingData> _types = new();
        private readonly List<ITileLayer> _allTileLayers = new();
        private readonly Dictionary<ITileLayer, TileLayerMetaData> _tileLayerMetaData = new();
        private List<ITileLayer> _enabledTileLayers = new();

        private IMap? _map;

        public void Initialize()
        {
            _tileAtlasManager.ThemeSwitched += OnThemeSwitched;
        }

        public void RegisterTileLayers()
        {
            _allTileLayers.Clear();
            _tileLayerMetaData.Clear();
            _enabledTileLayers.Clear();
            _types.Clear();

            _layerDependencyCollection = new(_entitySystemManager.DependencyCollection);

            foreach (var type in _reflectionManager.FindTypesWithAttribute<RegisterTileLayerAttribute>())
            {
                RegisterTileLayer(type);
            }

            _layerDependencyCollection.BuildGraph();

            foreach (var type in GetSortedLayers())
            {
                var attr = type.GetCustomAttribute<RegisterTileLayerAttribute>()!;
                var layer = (ITileLayer) _layerDependencyCollection.ResolveType(type);
                layer.Initialize();
                _allTileLayers.Add(layer);
                _tileLayerMetaData.Add(layer, new TileLayerMetaData(enabled: attr.EnabledAtStartup));
            }

            if (_map != null)
            {
                foreach (var layer in _allTileLayers)
                {
                    layer.SetMap(_map);
                }
            }

            RebuildEnabledTileLayers();
        }

        private void RebuildEnabledTileLayers()
        {
            // _tileLayers should be sorted according to the ordering data by now.
            _enabledTileLayers = _allTileLayers
                .Where(tileLayer => _tileLayerMetaData[tileLayer].Enabled)
                .ToList();
        }

        public void SetTileLayerEnabled<T>(bool enabled) where T : ITileLayer
            => SetTileLayerEnabled(typeof(T), enabled);

        public void SetTileLayerEnabled(Type type, bool enabled)
        {
            var tileLayer = _allTileLayers.Where(x => x.GetType() == type).FirstOrDefault();
            if (tileLayer == null)
                return;

            var meta = _tileLayerMetaData[tileLayer];
            meta.Enabled = enabled;

            RebuildEnabledTileLayers();
        }

        private void RegisterTileLayer(Type type)
        {
            Logger.InfoS(Sawmill, "Registering tile layer {0}", type);

            if (!typeof(ITileLayer).IsAssignableFrom(type))
            {
                Logger.ErrorS(Sawmill, "Type {0} has RegisterComponentAttribute but does not implement IComponent.", type);
            }

            if (_types.ContainsKey(type))
            {
                throw new InvalidOperationException($"Type is already registered: {type}");
            }

            var attribute = type.GetCustomAttribute<RegisterTileLayerAttribute>()!;
            _types.Add(type, new OrderingData(type, attribute.RenderBefore, attribute.RenderAfter));
            _layerDependencyCollection.Register(type);
        }

        public T GetTileLayer<T>() where T : ITileLayer
        {
            return (T)_allTileLayers.Single(layer => layer is T);
        }

        private IEnumerable<Type> GetSortedLayers()
        {
            var nodes = TopologicalSort.FromBeforeAfter(
                _types.Values,
                n => n.OrderType,
                n => n,
                n => n.Before ?? Array.Empty<Type>(),
                n => n.After ?? Array.Empty<Type>(),
                allowMissing: true);

            return TopologicalSort.Sort(nodes)
                .Select(o => o.OrderType);
        }

        public void SetMap(IMap map)
        {
            _map = map;

            foreach (var layer in _allTileLayers)
            {
                layer.SetMap(map);
            }

            map.RedrawAllThisTurn = true;
            RefreshAllLayers();
            _mapDrawables.Clear();
        }

        private void OnThemeSwitched()
        {
            if (_map == null)
                return;

            foreach (var layer in _allTileLayers)
            {
                layer.OnThemeSwitched();
            }

            foreach (var layer in _allTileLayers)
            {
                layer.RedrawAll();
            }
        }

        public void RefreshAllLayers()
        {
            if (_map == null)
                return;

            if (_map.RedrawAllThisTurn)
            {
                foreach (var layer in _allTileLayers)
                {
                    layer.RedrawAll();
                }
            }
            else if(_map.DirtyTilesThisTurn.Count > 0)
            {
                foreach (var layer in _allTileLayers)
                {
                    layer.RedrawDirtyTiles(_map.DirtyTilesThisTurn);
                }
            }

            _map.RedrawAllThisTurn = false;
            _map.DirtyTilesThisTurn.Clear();
            _map.MapObjectMemory.Flush();
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            foreach (var layer in _allTileLayers)
            {
                layer.SetSize(width, height);
            }
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            foreach (var layer in _allTileLayers)
            {
                layer.SetPosition(x, y);
            }
            _mapDrawables.SetPosition(x, y);
        }

        public override void Update(float dt)
        {
            foreach (var layer in _allTileLayers)
            {
                layer.Update(dt);
            }
            _mapDrawables.Update(dt);
        }

        public override void Draw()
        {
            foreach (var layer in _enabledTileLayers)
            {
                layer.Draw();
            }
            _mapDrawables.Draw();
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
