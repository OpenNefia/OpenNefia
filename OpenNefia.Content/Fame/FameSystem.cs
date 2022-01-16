using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Fame
{
    public sealed class FameSystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<FameComponent, EntityRefreshEvent>(HandleRefresh, nameof(HandleRefresh));
        }

        private void HandleRefresh(EntityUid uid, FameComponent fameComp, ref EntityRefreshEvent args)
        {
            fameComp.Fame.Reset();
        }
    }
}
