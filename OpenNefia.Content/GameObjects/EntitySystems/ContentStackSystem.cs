using OpenNefia.Content.Logic;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
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
        [Dependency] private readonly IMessagesManager _mes = default!;
        public override void Initialize()
        {
            SubscribeEntity<EntityStackedEvent>(HandleStacked);
        }

        private void HandleStacked(EntityUid uid, ref EntityStackedEvent args)
        {
            if (args.ShowMessage)
            {
                _mes.Display(Loc.GetString("Elona.GameObjects.Stack.HasBeenStacked",
                    ("entity", uid),
                    ("totalCount", args.NewCount)));
            }
        }
    }
}
