using OpenNefia.Content.Effects;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Religion
{
    [DataDefinition]
    public sealed class GodItem
    {
        [DataField]
        public PrototypeId<EntityPrototype> ItemId { get; }

        [DataField]
        public bool OnlyOnce { get; }

        // TODO item filter
        [DataField]
        public bool NoStack { get; }
    }

    [DataDefinition]
    public sealed class GodOffering
    {
        // TODO one or the other
        [DataField]
        public PrototypeId<TagPrototype>? Category { get; }

        [DataField]
        public PrototypeId<EntityPrototype>? ItemId { get; }
    }

    [ImplicitDataDefinitionForInheritors]
    public interface IGodCallbacks
    {
        void OnJoinFaith(EntityUid target);
        void OnLeaveFaith(EntityUid target);
    }

    [Prototype("Elona.God")]
    public class GodPrototype : IPrototype, IHspIds<int>
    {
        /// <inheritdoc />
        [IdDataField]
        public string ID { get; private set; } = default!;

        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        /// <summary>
        /// If <c>true</c>, altars to this god can be randomly generated
        /// and characters can be spawned worshipping this god.
        /// Custom gods in other versions of Elona behave the same
        /// as when this flag is <c>false</c>.
        /// </summary>
        /// <seealso cref="IReligionSystem.PickRandomGodID(bool)"/>
        [DataField]
        public bool CanAppearRandomly { get; } = false;

        /// <summary>
        /// Entity this god uses when they are summoned through wishing.
        /// </summary>
        [DataField]
        public PrototypeId<EntityPrototype>? Summon { get; }

        /// <summary>
        /// Entity offered by this god as a servant.
        /// </summary>
        [DataField]
        public PrototypeId<EntityPrototype>? Servant { get; }

        [DataField("items")]
        private readonly List<GodItem> _items = new();
        public IReadOnlyList<GodItem> Items => _items;

        [DataField("offerings")]
        private readonly List<GodOffering> _offerings = new();
        public IReadOnlyList<GodOffering> Offerings => _offerings;

        /// <summary>
        /// Entity offered by this god as an artifact.
        /// </summary>
        [DataField]
        public PrototypeId<EntityPrototype>? Artifact { get; }

        [DataField]
        public IEffect? Blessing { get; }

        [DataField]
        public IGodCallbacks? Callbacks { get; }
    }
}
