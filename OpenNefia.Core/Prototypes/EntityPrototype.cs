using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace OpenNefia.Core.Prototypes
{
    /// <summary>
    /// Prototype that represents game entities.
    /// </summary>
    [Prototype("Entity", -1)]
    public class EntityPrototype : IPrototype, IInheritingPrototype, ISerializationHooks, IHspIds<int>
    {
        /// <summary>
        /// The "in code name" of the object. Must be unique.
        /// </summary>
        [DataField("id")]
        public string ID { get; private set; } = default!;

        /// <summary>
        /// Type identifier for the corresponding entity in HSP variants of Elona.
        /// 
        /// Since all types of entities (character, item, feat, mef) are consolidated
        /// under <see cref="EntityPrototype"/>, it becomes necessary to state both what
        /// the "logical" type ("chara", "item") and the numeric ID in <see cref="HspIds"/> are.
        /// </summary>
        /// <seealso cref="HspEntityTypes"/>
        [DataField]
        public string? HspEntityType { get; }

        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        /// <summary>
        /// Cell object IDs of this entity for each HSP variant.
        /// This is used during HSP map conversion.
        /// </summary>
        [DataField("hspCellObjIds")]
        [NeverPushInheritance]
        private readonly Dictionary<string, HashSet<int>> _hspCellObjIds = new();
        public IReadOnlyDictionary<string, HashSet<int>> HspCellObjIds => _hspCellObjIds;

        /// <summary>
        ///     If true, this object should not show up in the entity spawn panel.
        /// </summary>
        [NeverPushInheritance]
        [DataField]
        public bool Abstract { get; private set; }

        /// True if this entity will be saved by the map loader.
        /// </summary>
        [DataField]
        public bool MapSavable { get; protected set; } = true;

        /// <summary>
        /// The prototype we inherit from.
        /// </summary>
        [DataField(customTypeSerializer: typeof(PrototypeIdStringSerializer<EntityPrototype>))]
        public string? Parent { get; private set; }

        /// <summary>
        /// A dictionary mapping the component type list to the YAML mapping containing their settings.
        /// </summary>
        [DataField]
        [AlwaysPushInheritance]
        public ComponentRegistry Components { get; } = new();

        public EntityPrototype()
        {
            // All entities come with a spatial and metadata component.
            Components.Add("Spatial", new SpatialComponent());
            Components.Add("MetaData", new MetaDataComponent());
        }

        public bool TryGetComponent<T>(string name, [NotNullWhen(true)] out T? component) where T : IComponent
        {
            if (!Components.TryGetValue(name, out var componentUnCast))
            {
                component = default;
                return false;
            }

            // There are no duplicate component names
            // TODO Sanity check with names being in an attribute of the type instead
            component = (T)componentUnCast;
            return true;
        }

        public override string ToString()
        {
            return $"EntityPrototype({ID})";
        }

        public class ComponentRegistry : Dictionary<string, IComponent>
        {
            public ComponentRegistry()
            {
            }

            public ComponentRegistry(Dictionary<string, IComponent> components) : base(components)
            {
            }

            public bool HasComponent<T>()
                where T : class, IComponent
            {
                var dummy = (IComponent)Activator.CreateInstance(typeof(T))!;

                if (!TryGetValue(dummy.Name, out var comp))
                    return false;

                return comp is T;
            }

            public bool TryGetComponent<T>([NotNullWhen(true)] out T? component)
                where T : class, IComponent
            {
                var dummy = (IComponent)Activator.CreateInstance(typeof(T))!;

                if (TryGetValue(dummy.Name, out var comp) && comp is T)
                {
                    component = (comp as T)!;
                    return true;
                }

                component = null;
                return false;
            }

            public T GetComponent<T>()
                where T : class, IComponent
            {
                var dummy = (IComponent)Activator.CreateInstance(typeof(T))!;
                return (this[dummy.Name] as T)!;
            }
        }
    }
}
