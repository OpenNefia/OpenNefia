using System;
using OpenNefia.Core.Audio;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.Audio
{
    [TypeSerializer]
    public class SoundSpecifierTypeSerializer :
        ITypeReader<SoundSpecifier, MappingDataNode>,
        ITypeReader<SoundSpecifier, ValueDataNode>
    {
        private Type GetType(MappingDataNode node)
        {
            var hasPath = node.Has(SoundPathSpecifier.Node);
            var hasCollection = node.Has(SoundCollectionSpecifier.Node);

            if (hasPath || !(hasPath ^ hasCollection))
                return typeof(SoundPathSpecifier);

            if (hasCollection)
                return typeof(SoundCollectionSpecifier);

            return typeof(SoundPathSpecifier);
        }

        public SoundSpecifier Read(ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null,
            SoundSpecifier? rawValue = null)
        {
            var type = GetType(node);
            return (SoundSpecifier)serializationManager.Read(type, node, context, skipHook)!;
        }

        public SoundSpecifier Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null,
            SoundSpecifier? rawValue = null)
        {
            return new SoundPathSpecifier(new(node.Value));
        }

        public ValidationNode Validate(ISerializationManager serializationManager, MappingDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            if (node.Has(SoundPathSpecifier.Node) && node.Has(SoundCollectionSpecifier.Node))
                return new ErrorNode(node, "You can only specify either a sound path or a sound collection!");

            if (!node.Has(SoundPathSpecifier.Node) && !node.Has(SoundCollectionSpecifier.Node))
                return new ErrorNode(node, "You need to specify either a sound path or a sound collection!");

            return serializationManager.ValidateNode(GetType(node), node, context);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            if (serializationManager.ValidateNode<PrototypeId<SoundPrototype>>(node, context) is not ErrorNode)
                return new ValidatedValueNode(node);

            return new ErrorNode(node, $"SoundSpecifier value is not a valid {nameof(SoundPrototype)} ID!");
        }
    }
}
