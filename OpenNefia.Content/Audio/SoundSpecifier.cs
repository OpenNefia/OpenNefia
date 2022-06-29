using OpenNefia.Core.Audio;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations;

namespace OpenNefia.Content.Audio
{
    [ImplicitDataDefinitionForInheritors]
    public abstract class SoundSpecifier
    {
        public abstract PrototypeId<SoundPrototype>? GetSound();
    }

    public sealed class SoundNullSpecifier : SoundSpecifier
    {
        public override PrototypeId<SoundPrototype>? GetSound() => null;
    }

    public sealed class SoundPathSpecifier : SoundSpecifier
    {
        public const string Node = "soundID";

        [DataField(Node, required: true)]
        public PrototypeId<SoundPrototype>? SoundID { get; }

        public SoundPathSpecifier()
        {
        }

        public SoundPathSpecifier(PrototypeId<SoundPrototype> soundID)
        {
            SoundID = soundID;
        }

        public override PrototypeId<SoundPrototype>? GetSound()
        {
            return SoundID;
        }
    }

    public sealed class SoundCollectionSpecifier : SoundSpecifier
    {
        public const string Node = "collection";

        [DataField(Node, required: true)]
        public PrototypeId<SoundCollectionPrototype>? Collection { get; }

        public SoundCollectionSpecifier()
        {
        }

        public SoundCollectionSpecifier(PrototypeId<SoundCollectionPrototype> collection)
        {
            Collection = collection;
        }

        public override PrototypeId<SoundPrototype>? GetSound()
        {
            return Collection == null ? null : AudioHelpers.GetRandomFileFromSoundCollection(Collection.Value);
        }
    }
}
