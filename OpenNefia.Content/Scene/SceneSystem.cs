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
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Log;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Content.Scene
{
    public interface ISceneSystem : IEntitySystem
    {
        void PlayScene(PrototypeId<ScenePrototype> sceneID);
    }

    public sealed class SceneSystem : EntitySystem, ISceneSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ISerializationManager _serialization = default!;
        [Dependency] private readonly IUserInterfaceManager _uiMan = default!;

        public override void Initialize()
        {
        }

        public void PlayScene(PrototypeId<ScenePrototype> sceneID)
        {
            if (!_protos.TryIndex(sceneID, out var sceneProto))
            {
                Logger.ErrorS("scene", $"Scene prototype {sceneID} doesn't exist!");
                return;
            }

            var rootPath = sceneProto.Location;
            var path = rootPath / Loc.Language.ToString() / sceneProto.Filename ;

            if (!_resourceCache.ContentFileExists(path))
            {
                var fallbackPath = rootPath / sceneProto.FallbackLanguage.ToString() / sceneProto.Filename ;
                if (!_resourceCache.ContentFileExists(fallbackPath))
                {
                    Logger.ErrorS("scene", $"Scene YAML file {path} doesn't exist!");
                    return;
                }
                else
                {
                    Logger.ErrorS("scene", $"Scene YAML file {path} doesn't exist, falling back to {fallbackPath}");
                    path = fallbackPath;
                }
            }

            try
            {
                var sceneYaml = _resourceCache.ContentFileReadYaml(path);
                var rootNode = sceneYaml.Documents[0].RootNode.ToDataNodeCast<MappingDataNode>();
                var sceneFile = _serialization.Read<SceneFile>(rootNode);

                var args = new SceneLayer.Args(sceneFile.Nodes);
                _uiMan.Query<SceneLayer, SceneLayer.Args>(args);
            }
            catch (Exception ex)
            {
                Logger.ErrorS("scene", ex, $"Failed to load scene YAML file {path}");
            }
        }
    }
}