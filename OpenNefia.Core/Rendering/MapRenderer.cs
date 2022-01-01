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

        [Dependency] private readonly IMapDrawables _mapDrawables = default!;
        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly ITileAtlasManager _tileAtlasManager = default!;
        [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

        private ISawmill _sawmill = default!;
        private DependencyCollection _layerDependencyCollection = default!;

        private sealed record OrderingData(Type OrderType, Type[]? Before, Type[]? After);

        private Dictionary<Type, OrderingData> _types = new();
        private List<ITileLayer> _tileLayers = new();
        private IMap? _map;

        public void Initialize()
        {
            _tileAtlasManager.ThemeSwitched += OnThemeSwitched;
        }

        public void RegisterTileLayers()
        {
            _tileLayers.Clear();
            _types.Clear();

            _layerDependencyCollection = new(_entitySystemManager.DependencyCollection);

            foreach (var type in _reflectionManager.FindTypesWithAttribute<RegisterTileLayerAttribute>())
            {
                RegisterTileLayer(type);
            }

            _layerDependencyCollection.BuildGraph();

            foreach (var type in GetSortedLayers())
            {
                var layer = (ITileLayer) _layerDependencyCollection.ResolveType(type);
                layer.Initialize();
                _tileLayers.Add(layer);
            }

            GetSortedLayers();

            if (_map != null)
            {
                foreach (var layer in _tileLayers)
                {
                    layer.SetMap(_map);
                }
            }
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

            foreach (var layer in _tileLayers)
            {
                layer.SetMap(map);
            }

            map.RedrawAllThisTurn = true;
            RefreshAllLayers();
            _mapDrawables.Clear();
        }

        private void OnThemeSwitched()
        {
            if (this._map == null)
                return;

            foreach (var layer in _tileLayers)
            {
                layer.OnThemeSwitched();
            }

            foreach (var layer in _tileLayers)
            {
                layer.RedrawAll();
            }
        }

        public void RefreshAllLayers()
        {
            if (this._map == null)
                return;

            if (this._map.RedrawAllThisTurn)
            {
                foreach (var layer in _tileLayers)
                {
                    layer.RedrawAll();
                }
            }
            else if(this._map.DirtyTilesThisTurn.Count > 0)
            {
                foreach (var layer in _tileLayers)
                {
                    layer.RedrawDirtyTiles(this._map.DirtyTilesThisTurn);
                }
            }

            this._map.RedrawAllThisTurn = false;
            this._map.DirtyTilesThisTurn.Clear();
            this._map.MapObjectMemory.Flush();
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            foreach (var layer in this._tileLayers)
            {
                layer.SetSize(width, height);
            }
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            foreach (var layer in this._tileLayers)
            {
                layer.SetPosition(x, y);
            }
            _mapDrawables.SetPosition(x, y);
        }

        public override void Update(float dt)
        {
            foreach (var layer in _tileLayers)
            {
                layer.Update(dt);
            }
            _mapDrawables.Update(dt);
        }

        public override void Draw()
        {
            foreach (var layer in _tileLayers)
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
