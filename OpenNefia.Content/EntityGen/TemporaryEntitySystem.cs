using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.EntityGen
{
    /// <summary>
    /// Operations for "temporary" entities, defined as:
    /// <list type="bullet">
    /// <item>Entity is spawned into the <see cref="MapId.Global"/> map.</item>
    /// <item>Entity has the <see cref="MetaDataComponent.IsMapSavable"/> flag set to <c>false</c>.</item>
    /// </list>
    /// This is useful for times when you need the player to pick an entity from a list and cleanup the
    /// entities that were not picked afterwards.
    /// </summary>
    public interface ITemporaryEntitySystem : IEntitySystem
    {
        void ClearGlobalTemporaryEntities();
    }

    public sealed class TemporaryEntitySystem : EntitySystem, ITemporaryEntitySystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public void ClearGlobalTemporaryEntities()
        {
            foreach (var meta in _lookup.EntityQueryInMap<MetaDataComponent>(MapId.Global))
            {
                if (meta.IsMapSavable == false)
                    EntityManager.DeleteEntity(meta.Owner, EntityDeleteType.Delete);
            }
        }
    }
}