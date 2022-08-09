using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Inventory
{
    public class ContainerInvSource : IInventorySource
    {
        public IContainer Container { get; }

        public ContainerInvSource(IContainer container)
        {
            Container = container;
        }

        public IEnumerable<EntityUid> GetEntities()
        {
            return Container.ContainedEntities;
        }

        public void OnDraw()
        {
        }
    }
}
