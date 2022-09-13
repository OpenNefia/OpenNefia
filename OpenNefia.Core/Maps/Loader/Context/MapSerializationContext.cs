using JetBrains.Annotations;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using OpenNefia.Core.Utility;
using System.Globalization;

namespace OpenNefia.Core.Maps
{
    internal class MapSerializationContext : ISerializationContext, IEntityLoadContext,
            ITypeSerializer<EntityUid, ValueDataNode>,
            ITypeReaderWriter<EntityUid, ValueDataNode>
    {
        private readonly MapSerializeMode _mode;
        private readonly ISerializationManager _serializationManager;

        public Dictionary<string, MappingDataNode>? CurrentReadingEntityComponents;
        public HashSet<string>? CurrentDeletedEntityComponents;

        public readonly Dictionary<EntityUid, int> EntityUidMap = new();
        public readonly Dictionary<int, EntityUid> UidEntityMap = new();
        public readonly List<EntityUid> Entities = new();

        public Dictionary<(Type, Type), object> TypeReaders { get; }
        public Dictionary<Type, object> TypeWriters { get; }
        public Dictionary<Type, object> TypeCopiers => TypeWriters;
        public Dictionary<(Type, Type), object> TypeValidators => TypeReaders;
        public Dictionary<Type, object> TypeComparers => TypeWriters;

        public bool EnsurePrototypeComponents => _mode == MapSerializeMode.Blueprint;

        public MapSerializationContext(MapSerializeMode mode, ISerializationManager serializationManager)
        {
            _mode = mode;
            _serializationManager = serializationManager;

            TypeWriters = new Dictionary<Type, object>()
                {
                    {typeof(EntityUid), this}
                };
            TypeReaders = new Dictionary<(Type, Type), object>()
                {
                    {(typeof(EntityUid), typeof(ValueDataNode)), this}
                };
        }

        // Create custom object serializers that will correctly allow data to be overriden by the map file.
        MappingDataNode IEntityLoadContext.GetComponentData(string componentName,
            MappingDataNode? protoData)
        {
            if (CurrentReadingEntityComponents == null)
            {
                throw new InvalidOperationException();
            }

            var factory = IoCManager.Resolve<IComponentFactory>();
            var serializationManager = IoCManager.Resolve<ISerializationManager>();

            if (CurrentReadingEntityComponents.TryGetValue(componentName, out var mapping))
            {
                if (protoData == null) return mapping.Copy();

                return serializationManager.PushCompositionWithGenericNode(
                    factory.GetRegistration(componentName).Type, new[] { protoData }, mapping, this);
            }

            return protoData ?? new MappingDataNode();
        }

        IEnumerable<string> IEntityLoadContext.GetExtraComponentTypes()
        {
            return CurrentReadingEntityComponents!.Keys;
        }

        bool IEntityLoadContext.ShouldLoadComponent(string componentName)
        {
            return !CurrentDeletedEntityComponents!.Contains(componentName);
        }

        ValidationNode ITypeValidator<EntityUid, ValueDataNode>.Validate(ISerializationManager serializationManager,
            ValueDataNode node, IDependencyCollection dependencies, ISerializationContext? context)
        {
            if (node.Value == "null")
            {
                return new ValidatedValueNode(node);
            }

            if (!int.TryParse(node.Value, out var val))
            {
                return new ErrorNode(node, $"Could not parse {nameof(EntityUid)}", true);
            }

            if (_mode == MapSerializeMode.Blueprint && !UidEntityMap.ContainsKey(val))
            {
                return new ErrorNode(node, $"{nameof(EntityUid)} {val} was not found in entity list of map blueprint.", true);
            }

            return new ValidatedValueNode(node);
        }

        public DataNode Write(ISerializationManager serializationManager, EntityUid value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            var entityUid = (int)value;

            if (_mode == MapSerializeMode.Blueprint)
            {
                if (!EntityUidMap.TryGetValue(value, out var entityUidMapped))
                {
                    Logger.WarningS(MapLoader.SawmillName, "Cannot write entity UID '{0}'.", value);
                    return new ValueDataNode("null");
                }
                entityUid = entityUidMapped;
            }

            return new ValueDataNode(entityUid.ToString(CultureInfo.InvariantCulture));
        }

        EntityUid ITypeReader<EntityUid, ValueDataNode>.Read(ISerializationManager serializationManager,
            ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context,
            EntityUid rawValue = default)
        {
            if (node.Value == "null")
            {
                Logger.ErrorS(MapLoader.SawmillName, $"Found null {nameof(EntityUid)} in map file.");
                return EntityUid.Invalid;
            }

            var val = int.Parse(node.Value);

            if (_mode == MapSerializeMode.Blueprint)
            {
                if (val >= Entities.Count
                || !UidEntityMap.ContainsKey(val)
                || !Entities.TryFirstOrNull(e => e == UidEntityMap[val], out var entity))
                {
                    Logger.ErrorS(MapLoader.SawmillName, $"Error in map blueprint file: found local entity UID '{val}' which does not exist.");
                    return EntityUid.Invalid;
                }
                else
                {
                    return entity!.Value;
                }
            }
            else
            {
                return new EntityUid(val);
            }
        }

        [MustUseReturnValue]
        public EntityUid Copy(ISerializationManager serializationManager, EntityUid source, EntityUid target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new((int)source);
        }

        public bool Compare(ISerializationManager serializationManager, EntityUid left, EntityUid right,
            bool skipHook, ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}