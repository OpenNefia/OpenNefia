using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Rendering.TileDrawLayers;
using OpenNefia.Core.Rendering.TileRowDrawLayers;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public sealed class MapTileRowRenderer : IMapTileRowRenderer
    {
        private const string Sawmill = "maprenderer.sys.row";

        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly ITileAtlasManager _tileAtlasManager = default!;
        [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

        private sealed record OrderingData(TileRowLayerType Type, Type OrderType, Type[]? Before, Type[]? After);

        private sealed class TileRowLayerMetaData
        {
            public bool Enabled { get; set; }

            public TileRowLayerMetaData(bool enabled)
            {
                Enabled = enabled;
            }
        }

        private IMap? _map;

        // Tile renderers for the main tilemap that render in "strips" so that they are correctly ordered behind wall overhangs/shadows
        private readonly Dictionary<Type, OrderingData> _rowTypes = new();
        private readonly List<(ITileRowLayer, TileRowLayerType)> _allTileRowLayers = new();
        private DependencyCollection _rowLayerDependencyCollection = default!;
        private readonly Dictionary<ITileRowLayer, TileRowLayerMetaData> _tileRowLayerMetaData = new();

        public IEnumerable<ITileRowLayer> GetTileRowLayers(TileRowLayerType type)
        {
            return _allTileRowLayers.Where(x => x.Item2 == type).Select(x => x.Item1);
        }

        public bool TryGetTileRowLayer<T>([NotNullWhen(true)] out T? layer)
            where T: class, ITileRowLayer
        {
            var result = _allTileRowLayers.TryFirstOrNull(x => x.Item1 is T, out var pair);
            if (pair != null)
            {
                layer = (T)pair.Value.Item1;
                return result;
            }
            layer = null;
            return false;
        }

        public void RegisterTileLayers()
        {
            _allTileRowLayers.Clear();
            _rowTypes.Clear();

            _rowLayerDependencyCollection = new(_entitySystemManager.DependencyCollection);

            foreach (var type in _reflectionManager.FindTypesWithAttribute<RegisterTileRowLayerAttribute>())
            {
                RegisterTileRowLayer(type, _rowTypes, _rowLayerDependencyCollection);
            }

            _rowLayerDependencyCollection.BuildGraph();

            foreach (var type in GetSortedLayers(_rowTypes))
            {
                var attr = type.GetCustomAttribute<RegisterTileRowLayerAttribute>()!;
                var layer = (ITileRowLayer)_rowLayerDependencyCollection.ResolveType(type);
                layer.Initialize();
                _allTileRowLayers.Add((layer, attr.Type));
                _tileRowLayerMetaData.Add(layer, new TileRowLayerMetaData(enabled: attr.EnabledAtStartup));
            }

            if (_map != null)
            {
                foreach (var (layer, _) in _allTileRowLayers)
                {
                    layer.SetMap(_map);
                }
            }
        }

        private IEnumerable<Type> GetSortedLayers(Dictionary<Type, OrderingData> types)
        {
            var nodes = TopologicalSort.FromBeforeAfter(
                types.Values,
                n => n.OrderType,
                n => n,
                n => n.Before ?? Array.Empty<Type>(),
                n => n.After ?? Array.Empty<Type>(),
                allowMissing: true);

            return TopologicalSort.Sort(nodes)
                .Select(o => o.OrderType);
        }

        private void RegisterTileRowLayer(Type type, Dictionary<Type, OrderingData> types, DependencyCollection dependencyCollection)
        {
            Logger.InfoS(Sawmill, "Registering tile layer {0}", type);

            if (!typeof(ITileRowLayer).IsAssignableFrom(type))
            {
                Logger.ErrorS(Sawmill, $"Type {type} has {nameof(RegisterTileRowLayerAttribute)} but does not implement {nameof(ITileRowLayer)}.");
            }

            if (types.ContainsKey(type))
            {
                throw new InvalidOperationException($"Type is already registered: {type}");
            }

            var attribute = type.GetCustomAttribute<RegisterTileRowLayerAttribute>()!;
            types.Add(type, new OrderingData(attribute.Type, type, attribute.RenderBefore, attribute.RenderAfter));
            dependencyCollection.Register(type);
        }

        public void SetMap(IMap map)
        {
            _map = map;

            foreach (var (layer, _) in _allTileRowLayers)
            {
                layer.SetMap(map);
            }

            RefreshAllLayers();
        }

        public void OnThemeSwitched()
        {
            if (_map == null)
                return;

            foreach (var (layer, _) in _allTileRowLayers)
            {
                layer.OnThemeSwitched();
            }

            foreach (var (layer, _) in _allTileRowLayers)
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
                foreach (var (layer, _) in _allTileRowLayers)
                {
                    layer.RedrawAll();
                }
            }
            else if (_map.DirtyTilesThisTurn.Count > 0)
            {
                foreach (var (layer, _) in _allTileRowLayers)
                {
                    layer.RedrawDirtyTiles(_map.DirtyTilesThisTurn);
                }
            }
        }
    }
}
