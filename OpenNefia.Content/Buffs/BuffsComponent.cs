using OpenNefia.Content.Enchantments;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Buffs
{
    [RegisterComponent]
    public class BuffsComponent : Component, ISerializationHooks
    {
        public static readonly ContainerId ContainerIdBuffs = new("Elona.Buffs");

        /// <summary>
        /// Contains the enchantment entities on this item.
        /// </summary>
        public Container Container { get; private set; } = default!;

        protected override void Initialize()
        {
            base.Initialize();

            Container = ContainerHelpers.EnsureContainer<Container>(Owner, ContainerIdBuffs);
        }

        bool ISerializationHooks.AfterCompare(object? other)
        {
            if (other is not EnchantmentsComponent otherEnc)
                return false;

            // Don't stack if either container is not empty.
            if (Container.ContainedEntities.Count > 0 || otherEnc.Container.ContainedEntities.Count > 0)
            {
                return false;
            }

            return true;
        }
    }
}
