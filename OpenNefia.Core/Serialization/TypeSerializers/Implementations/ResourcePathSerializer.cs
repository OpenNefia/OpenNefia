using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations
{
    public abstract class BaseResourcePathSerializer : ITypeSerializer<ResourcePath, ValueDataNode>
    {
        protected abstract string ValidationError { get; }

        public ResourcePath Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null, ResourcePath? value = default)
        {
            return new ResourcePath(node.Value);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            var path = new ResourcePath(node.Value);

            path = path.ToRootedPath();

            try
            {
                return ValidatePath(path)
                    ? new ValidatedValueNode(node)
                    : new ErrorNode(node, $"{ValidationError}. ({path})");
            }
            catch (Exception e)
            {
                return new ErrorNode(node, $"Failed parsing resource path. ({path}) ({e.Message})");
            }
        }

        protected abstract bool ValidatePath(ResourcePath path);

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

        public bool Compare(ISerializationManager serializationManager, ResourcePath left, ResourcePath right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }

    [TypeSerializer]
    public class ResourcePathSerializer : BaseResourcePathSerializer
    {
        protected override string ValidationError => "File not found.";

        protected override bool ValidatePath(ResourcePath path)
        {
            return IoCManager.Resolve<IResourceManager>().ContentFileExists(path);
        }
    }

    public class ResourcePathDirectorySerializer : BaseResourcePathSerializer
    {
        protected override string ValidationError => "Directory not found.";

        protected override bool ValidatePath(ResourcePath path)
        {
            // IResourceManager doesn't keep track of directories...
            return true;
        }
    }
}
