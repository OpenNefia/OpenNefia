using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Configuration;

namespace OpenNefia.Core.Rendering
{
    public class TileAtlasManager : ITileAtlasManager
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly ISerializationManager _serialization = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        public event Action? ThemeSwitched = null;

        private Dictionary<string, TileAtlas> _atlases = new();

        public void Initialize()
        {
            _prototypeManager.PrototypesReloaded += OnPrototypesReloaded;
        }

        private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
        {
            if (args.ByType != null && (args.ByType.ContainsKey(typeof(ChipPrototype)) || args.ByType.ContainsKey(typeof(TilePrototype))))
            {
                Logger.InfoS(CommonSawmills.ResAtlas, "Detected atlas prototype reload.");

                Clear();
                LoadAtlases();

                ThemeSwitched?.Invoke();
            }
        }

        public void LoadAtlases()
        {
            using (var sw = new ProfilerLogger(LogLevel.Info, CommonSawmills.ResAtlas, "Loading atlases"))
            {
                // IAtlasRegionProviders return one or more tiles assigned to an atlas,
                // where they will be packed into.
                var protos = GetAtlasRegionProviders();
                var regionsByAtlas = protos.SelectMany(proto => proto.GetAtlasRegions())
                    .GroupBy(region => region.atlasName);

                foreach (var group in regionsByAtlas)
                {
                    var atlasName = group.Key;
                    Logger.LogS(LogLevel.Info, CommonSawmills.ResAtlas, $"Loading atlas: {atlasName}");

                    // Assign the identifier used to refer to each tile in the atlas
                    // sprite batches.
                    foreach (var region in group)
                    {
                        region.spec.AtlasIndex = region.id;
                    
                        // True if this tile is a wall, for fancy wall rendering.
                        region.spec.HasOverhang = region.hasOverhang;
                    }

                    var atlas = new TileAtlasFactory(_resourceCache, _serialization, _config)
                        .LoadTiles(group.Select(x => x.spec))
                        .Build();

                    AddAtlas(atlasName, atlas);
                }
            }
        }

        private List<IAtlasRegionProvider> GetAtlasRegionProviders()
        {
            var protos = new List<IAtlasRegionProvider>();

            foreach (var type in _reflectionManager.GetAllChildren(typeof(IAtlasRegionProvider)))
            {
                if (typeof(IPrototype).IsAssignableFrom(type))
                {
                    protos.AddRange(_prototypeManager.EnumeratePrototypes(type).Cast<IAtlasRegionProvider>());
                }
            }

            return protos;
        }

        private void AddAtlas(string atlasName, TileAtlas atlas)
        {
            _atlases[atlasName] = atlas;
        }

        public TileAtlas GetAtlas(string atlasName)
        {
            return _atlases[atlasName];
        }

        private void Clear()
        {
            foreach (var atlas in _atlases.Values)
            {
                atlas.Dispose();
            }

            _atlases.Clear();
        }
    }
}
