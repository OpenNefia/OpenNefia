using System;
using System.Linq;
using JetBrains.Annotations;
using Why.Core.ContentPack;
using Why.Core.GameObjects;
using Why.Core.IoC;
using Why.Core.Serialization.Manager;
using Why.Core.Serialization.Manager.Attributes;
using Why.Core.Serialization.Manager.Result;
using Why.Core.Serialization.Markdown;
using Why.Core.Serialization.Markdown.Validation;
using Why.Core.Serialization.Markdown.Value;
using Why.Core.Serialization.TypeSerializers.Interfaces;
using Why.Core.Utility;

namespace Why.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class ResourcePathSerializer : ITypeSerializer<ResourcePath, ValueDataNode>
    {
        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new DeserializedValue<ResourcePath>(new ResourcePath(node.Value));
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            var path = new ResourcePath(node.Value);

            if (path.Extension.Equals("rsi"))
            {
                path /= "meta.json";
            }

            path = path.ToRootedPath();

            try
            {
                return IoCManager.Resolve<IResourceManager>().ContentFileExists(path)
                    ? new ValidatedValueNode(node)
                    : new ErrorNode(node, $"File not found. ({path})");
            }
            catch (Exception e)
            {
                return new ErrorNode(node, $"Failed parsing filepath. ({path}) ({e.Message})");
            }
        }

        public DataNode Write(ISerializationManager serializationManager, ResourcePath value,
            bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode(value.ToString());
        }

        [MustUseReturnValue]
        public ResourcePath Copy(ISerializationManager serializationManager, ResourcePath source, ResourcePath target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new(source.ToString());
        }
    }
}
