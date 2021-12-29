using OpenNefia.Content.Logic;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
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
            if (args.ShowMessage)
            {
                Mes.Display(Loc.GetString("Elona.GameObjects.Stack.HasBeenStacked",
                    ("entity", uid),
                    ("totalCount", args.NewCount)));
            }
        }
    }
}
