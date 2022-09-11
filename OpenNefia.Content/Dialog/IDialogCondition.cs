using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.UI;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Log;
using OpenNefia.Content.GameObjects;
using System.Diagnostics.Metrics;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Sidequests;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;

namespace OpenNefia.Content.Dialog
{
    /// <summary>
    /// Condition that returns the state of a sidequest.
    /// </summary>
    public sealed class SidequestStateCondition : IDialogCondition
    {
        [Dependency] private readonly ISidequestSystem _sidequests = default!;

        [DataField]
        public PrototypeId<SidequestPrototype> SidequestID { get; set; }

        public int GetValue(IDialogEngine engine)
        {
            EntitySystem.InjectDependencies(this);

            return _sidequests.GetState(SidequestID);
        }
    }

    /// <summary>
    /// Condition that counts the number of entities in the player's map with a given tag.
    /// </summary>
    public sealed class FindEntitiesWithTagCondition : IDialogCondition
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ITagSystem _tags = default!;

        [DataField]
        public PrototypeId<TagPrototype> Tag { get; set; }

        public int GetValue(IDialogEngine engine)
        {
            EntitySystem.InjectDependencies(this);

            var spatial = _entityManager.GetComponent<SpatialComponent>(engine.Player);
            return _tags.EntitiesWithTagInMap(spatial.MapID, Tag).Count();
        }
    }

    /// <summary>
    /// Condition that counts the number of entities with the given <see cref="EntityPrototype"/>.
    /// </summary>
    public sealed class FindEntitiesWithPrototypeCondition : IDialogCondition
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        [DataField]
        public PrototypeId<EntityPrototype> ProtoID { get; set; }

        public int GetValue(IDialogEngine engine)
        {
            EntitySystem.InjectDependencies(this);

            var spatial = _entityManager.GetComponent<SpatialComponent>(engine.Player);
            return _lookup.EntityQueryInMap<MetaDataComponent>(spatial.MapID)
                .Where(metadata => metadata.EntityPrototype?.GetStrongID() == ProtoID)
                .Count();
        }
    }
}