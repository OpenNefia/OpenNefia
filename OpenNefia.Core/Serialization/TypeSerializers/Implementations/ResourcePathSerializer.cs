using System;
using System.Linq;
using JetBrains.Annotations;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations
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
