using OpenNefia.Content.Logic;
using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    public class ContentStackSystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<SpatialComponent, EntityStackedEvent>(HandleStacked, nameof(HandleStacked));
        }

        private void HandleStacked(EntityUid uid, SpatialComponent component, ref EntityStackedEvent args)
        {
            Mes.Display($"{DisplayNameSystem.GetDisplayName(uid)} has been stacked (total: {args.NewCount})");
        }
    }
}
