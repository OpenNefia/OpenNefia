using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Audio
{
    [Prototype("Music")]
    public class MusicPrototype : IPrototype, IHspIds<int>
    {
        [IdDataField]
        public string ID { get; } = default!;
        
        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        [DataField(required: true)]
        public ResourcePath Filepath { get; } = default!;
    }
}