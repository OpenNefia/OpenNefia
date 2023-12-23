using OpenNefia.Content.Maps;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.SaveGames;
using OpenNefia.Content.TitleScreen;
using OpenNefia.Core.Log;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Areas;

namespace OpenNefia.Content.Areas
{
    public interface IGlobalAreaSystem : IEntitySystem
    {
        void InitializeGlobalAreas(IEnumerable<GlobalAreaId> globalAreaIds);
        IArea GetOrCreateGlobalArea(GlobalAreaId globalAreaId);

        /// <summary>
        /// Traverses the global area tree using breadth first search.
        /// Top-level areas are enumerated first, followed by children.
        /// </summary>
        /// <param name="globalAreaId">Root global area to traverse from. It is included in the resulting enumeration.</param>
        /// <returns>Enumerable of root/child global area IDs</returns>
        IEnumerable<GlobalAreaId> EnumerateGlobalAreaTree(GlobalAreaId globalAreaId);

        /// <summary>
        /// Enumerates the child and all parents of the global area graph.
        /// </summary>
        /// <param name="child">Node to start the enumeration at.</param>
        /// <returns>Enumerable of each successive parent. The passed child is included in the resulting enumeration.</returns>
        IEnumerable<GlobalAreaId> EnumerateGlobalAreaParents(GlobalAreaId child);

        /// <summary>
        /// Returns the root parent of a global area.
        /// </summary>
        /// <param name="child">Node to start the query at.</param>
        /// <returns>The root global area ID. Can be the same as the child if it is a root itself.</returns>
        GlobalAreaId? GetRootGlobalArea(GlobalAreaId child);
    }

    public sealed class GlobalAreaSystem : EntitySystem, IGlobalAreaSystem
    {
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IMapTransferSystem _mapTransfer = default!;
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;

        public record GlobalAreaEntry(GlobalAreaId GlobalAreaId, PrototypeId<EntityPrototype> AreaPrototypeId, GlobalAreaId? ParentId);

        private GlobalAreaGraph _globalAreas = new();

        private class GlobalAreaGraph
        {
            public Dictionary<GlobalAreaId, GlobalAreaEntry> Nodes = new();
            public Dictionary<GlobalAreaId, HashSet<GlobalAreaId>> Children = new();

            public void Clear()
            {
                Nodes.Clear();
                Children.Clear();
            }
        }

        public override void Initialize()
        {
            SubscribeBroadcast<BeforeNewGameStartedEventArgs>(InitializeGlobalAreas);

            InitializeGlobalAreaGraph();
        }

        private void InitializeGlobalAreaGraph()
        {
            _globalAreas.Clear();

            foreach (var entry in EnumerateGlobalAreaPrototypes())
            {
                Logger.DebugS("sys.globalAreas", $"Registering global area {entry.GlobalAreaId}, parent {entry.ParentId} (prototype: {entry.AreaPrototypeId})");

                if (_globalAreas.Nodes.ContainsKey(entry.GlobalAreaId))
                {
                    throw new InvalidDataException($"{entry.GlobalAreaId} already registered as a global area (prototype: {entry.AreaPrototypeId}");
                }

                _globalAreas.Nodes.Add(entry.GlobalAreaId, entry);
                if (entry.ParentId != null)
                {
                    var children = _globalAreas.Children.GetOrInsertNew(entry.ParentId.Value);
                    if (children.Contains(entry.GlobalAreaId))
                    {
                        throw new InvalidDataException($"{entry.GlobalAreaId} already registered as a child of global area {entry.ParentId.Value} (prototype: {entry.AreaPrototypeId}");
                    }
                    children.Add(entry.GlobalAreaId);
                }
            }

            try
            {
                // Topological sort will fail if the graph has cycles, which is not what we
                // want in this case.
                var nodes = TopologicalSort.FromBeforeAfter(_globalAreas.Nodes.Values,
                         p => p.GlobalAreaId,
                         p => p,
                         p => Array.Empty<GlobalAreaId>(),
                         p =>
                         {
                             if (p.ParentId != null)
                                 return new[] { p.ParentId.Value };
                             return Array.Empty<GlobalAreaId>();
                         },
                         allowMissing: true);
                TopologicalSort.Sort(nodes);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Global area graph contained cycles. Ensure there is at least one top-level area defined in the area prototypes.", ex);
            }
        }

        private void InitializeGlobalAreas(BeforeNewGameStartedEventArgs ev)
        {
            InitializeGlobalAreas(ev.Scenario.InitGlobalAreas);
        }

        /// <summary>
        /// Initializes a list of global areas and loads/saves their starting floors. For use with
        /// town/economy initialization when starting a new game.
        /// </summary>
        /// <remarks>
        /// This replicates some code called by *renew_economy in HSP
        /// </remarks>
        public void InitializeGlobalAreas(IEnumerable<GlobalAreaId> globalAreaIds)
        {
            var list = globalAreaIds.ToList();

            if (list.Count > 0)
            {
                Logger.DebugS("sys.globalAreas", "Initializing these global areas:");
                foreach (var globalAreaId in list)
                {
                    Logger.DebugS("sys.globalAreas", $"  - {globalAreaId}");
                }
            }

            foreach (var globalAreaId in list)
            {
                if (!_areaManager.TryGetGlobalArea(globalAreaId, out var area))
                {
                    area = GetOrCreateGlobalArea(globalAreaId);
                }
                if (area.ContainedMaps.Count == 0)
                {
                    // TODO might want to make this behavior configurable
                    InitializeStartingFloorOfArea(area);
                }
            }
        }

        /// <summary>
        /// Do an initialize-only load of a map to generate initial quests, etc.
        /// </summary>
        /// <param name="area"></param>
        private void InitializeStartingFloorOfArea(IArea area)
        {
            if (TryComp<AreaEntranceComponent>(area.AreaEntityUid, out var areaEntrance))
            {
                var map = _areaManager.GetOrGenerateMapForFloor(area.Id, areaEntrance.StartingFloor)!;
                if (map != null && TryComp<MapCommonComponent>(map.MapEntityUid, out var mapCommon) && !mapCommon.IsTemporary)
                {
                    _mapTransfer.RunMapInitializeEvents(map, MapLoadType.InitializeOnly);
                    _mapLoader.SaveMap(map.Id, _saveGameManager.CurrentSave!);
                    _mapManager.UnloadMap(map.Id);
                }
            }
        }

        public IArea GetOrCreateGlobalArea(GlobalAreaId globalAreaId)
        {
            if (_areaManager.TryGetGlobalArea(globalAreaId, out var area))
                return area;

            if (!_globalAreas.Nodes.TryGetValue(globalAreaId, out var entry))
                throw new InvalidDataException($"Global area '{globalAreaId}' was not defined in prototypes");

            AreaId? parentID = null;
            if (entry.ParentId != null)
            {
                if (!_areaManager.TryGetGlobalArea(entry.ParentId.Value, out var parentArea))
                    throw new InvalidDataException($"{globalAreaId} has a parent {entry.ParentId} that does not exist");
                parentID = parentArea?.Id;
            }

            area = _areaManager.CreateArea(entry.AreaPrototypeId, globalAreaId, parent: parentID);

            return area;
        }

        public IEnumerable<GlobalAreaId> EnumerateGlobalAreaTree(GlobalAreaId rootAreaId)
        {
            if (!_globalAreas.Nodes.ContainsKey(rootAreaId))
            {
                Logger.WarningS("sys.globalAreas", $"Unknown global area ID {rootAreaId} passed to {nameof(EnumerateGlobalAreaTree)}");
                yield break;
            }

            var nodes = new Queue<GlobalAreaId>();
            nodes.Enqueue(rootAreaId);

            do
            {
                var node = nodes.Dequeue();
                yield return node;

                if (!_globalAreas.Children.TryGetValue(node, out var children))
                    continue;

                foreach (var child in children)
                    nodes.Enqueue(child);
            } while (nodes.Count > 0);
        }

        public IEnumerable<GlobalAreaId> EnumerateGlobalAreaParents(GlobalAreaId childAreaId)
        {
            if (!_globalAreas.Nodes.TryGetValue(childAreaId, out var node))
            {
                Logger.WarningS("sys.globalAreas", $"Unknown global area ID {childAreaId} passed to {nameof(EnumerateGlobalAreaParents)}");
                yield break;
            }

            while (node.ParentId != null)
            {
                node = _globalAreas.Nodes[node.ParentId.Value];
                yield return node.GlobalAreaId;
            }
        }

        public GlobalAreaId? GetRootGlobalArea(GlobalAreaId childAreaId)
        {
            if (!_globalAreas.Nodes.TryGetValue(childAreaId, out var node))
            {
                Logger.WarningS("sys.globalAreas", $"Unknown global area ID {childAreaId} passed to {nameof(GetRootGlobalArea)}");
                return null;
            }

            var nodes = EnumerateGlobalAreaParents(childAreaId).ToList();

            if (nodes.Count == 0)
                return childAreaId;
            return nodes.Last();
        }

        private IEnumerable<GlobalAreaEntry> EnumerateGlobalAreaPrototypes()
        {
            foreach (var proto in _protos.EnumeratePrototypes<EntityPrototype>())
            {
                if (proto.Components.HasComponent<AreaComponent>()
                    && proto.Components.TryGetComponent<AreaEntranceComponent>(out var areaEntrance))
                {
                    if (areaEntrance.GlobalAreaSpec != null)
                    {
                        yield return new GlobalAreaEntry(areaEntrance.GlobalAreaSpec.ID, proto.GetStrongID(), areaEntrance.GlobalAreaSpec.Parent);
                    }
                }
            }
        }
    }
}