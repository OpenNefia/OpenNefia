using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Utility;
using YamlDotNet.RepresentationModel;
using static OpenNefia.Core.Prototypes.EntityPrototype;

namespace OpenNefia.Core.Maps
{
    internal sealed class MapValidator
    {
        [Dependency] private readonly IResourceManager _resourceManager = default!;
        [Dependency] private readonly IMapManagerInternal _mapManager = default!;
        [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
        [Dependency] private readonly IEntityManagerInternal _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly ISerializationManager _serializationManager = default!;

        private readonly MapSerializeMode _mode;
        private readonly YamlMappingNode _rootNode;
        private HashSet<ErrorNode> _errors = new();

        public Map? MapGrid { get; private set; }

        public MapValidator(MapSerializeMode mode, YamlMappingNode rootNode)
        {
            IoCManager.InjectDependencies(this);

            _mode = mode;
            _rootNode = rootNode;
        }

        public HashSet<ErrorNode> Validate()
        {
            _errors.Clear();

            // TODO validate tilemap
            ValidateEntities();

            return _errors;
        }

        private void ValidateEntities()
        {
            var uids = new HashSet<int>();
            var entities = _rootNode.GetNode<YamlSequenceNode>(MapLoadConstants.Entities);
            foreach (var entityDef in entities.Cast<YamlMappingNode>())
            {
                if (!entityDef.TryGetNode(MapLoadConstants.Entities_Uid, out var uidNode))
                {
                    _errors.Add(new ErrorNode(entityDef.ToDataNode(), $"Missing '{MapLoadConstants.Entities_Uid}' node in entity definition"));
                    continue;
                }
                if (!int.TryParse(uidNode.AsString(), out var uid))
                {
                    _errors.Add(new ErrorNode(entityDef.ToDataNode(), $"Could not parse entity UID: {uidNode.AsString()}"));
                    continue;
                }
                if (uids.Contains(uid))
                {
                    _errors.Add(new ErrorNode(entityDef.ToDataNode(), $"Duplicate entity UID: {uid}"));
                }

                uids.Add(uid);

                if (entityDef.TryGetNode(MapLoadConstants.Entities_ProtoId, out var typeNode))
                {
                    var protoId = new PrototypeId<EntityPrototype>(typeNode.AsString());
                    if (!_prototypeManager.HasIndex(protoId))
                    {
                        _errors.Add(new ErrorNode(typeNode.ToDataNode(), $"Missing entity prototype: {protoId}"));
                    }
                }

                if (entityDef.TryGetNode(MapLoadConstants.Entities_Components, out var componentsNode))
                {
                    var componentsResult = _serializationManager.ValidateNode<ComponentRegistry>(componentsNode.ToDataNode());
                    _errors.AddRange(componentsResult.GetErrors());
                }
            }
        }
    }
}
