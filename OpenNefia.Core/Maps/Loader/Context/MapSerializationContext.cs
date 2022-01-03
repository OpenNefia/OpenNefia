using JetBrains.Annotations;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Result;
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
        IComponent IEntityLoadContext.GetComponentData(string componentName,
            IComponent? protoData)
        {
            if (CurrentReadingEntityComponents == null)
            {
                throw new InvalidOperationException();
            }

            var factory = IoCManager.Resolve<IComponentFactory>();

            IComponent data = protoData != null
                ? _serializationManager.CreateCopy(protoData, this)!
                : (IComponent)Activator.CreateInstance(factory.GetRegistration(componentName).Type)!;

            if (CurrentReadingEntityComponents.TryGetValue(componentName, out var mapping))
            {
                var mapData = (IDeserializedDefinition)_serializationManager.Read(
                    factory.GetRegistration(componentName).Type,
                    mapping, this);
                var newData = _serializationManager.PopulateDataDefinition(data, mapData);
                data = (IComponent)newData.RawValue!;
            }

            return data;
        }

        IEnumerable<string> IEntityLoadContext.GetExtraComponentTypes()
        {
            return CurrentReadingEntityComponents!.Keys;
        }

        ValidationNode ITypeValidator<EntityUid, ValueDataNode>.Validate(ISerializationManager serializationManager,
            ValueDataNode node, IDependencyCollection dependencies, ISerializationContext? context)
        {
            if (node.Value == "null")
            {
                return new ValidatedValueNode(node);
            }

            if (!int.TryParse(node.Value, out var val) || !UidEntityMap.ContainsKey(val))
            {
                return new ErrorNode(node, $"Invalid {nameof(EntityUid)}", true);
            }

            return new ValidatedValueNode(node);
        }

        public DataNode Write(ISerializationManager serializationManager, EntityUid value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            if (!EntityUidMap.TryGetValue(value, out var _entityUidMapped))
            {
                Logger.WarningS(MapBlueprintLoader.SawmillName, "Cannot write entity UID '{0}'.", value);
                return new ValueDataNode("null");
            }
            else
            {
                return new ValueDataNode(_entityUidMapped.ToString(CultureInfo.InvariantCulture));
            }
        }

        DeserializationResult ITypeReader<EntityUid, ValueDataNode>.Read(ISerializationManager serializationManager,
            ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context)
        {
            if (node.Value == "null")
            {
                return new DeserializedValue<EntityUid>(EntityUid.Invalid);
            }

            var val = int.Parse(node.Value);

            if (val >= Entities.Count || !UidEntityMap.ContainsKey(val) || !Entities.TryFirstOrNull(e => e == UidEntityMap[val], out var entity))
            {
                Logger.ErrorS(MapBlueprintLoader.SawmillName, "Error in map file: found local entity UID '{0}' which does not exist.", val);
                return null!;
            }
            else
            {
                return new DeserializedValue<EntityUid>(entity!.Value);
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