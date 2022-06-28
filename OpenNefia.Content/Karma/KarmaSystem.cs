using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Karma;
using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Fame
{
    public sealed class KarmaSystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<KarmaComponent, EntityRefreshEvent>(HandleRefresh);
        }

        private void HandleRefresh(EntityUid uid, KarmaComponent karmaComp, ref EntityRefreshEvent args)
        {
            karmaComp.Karma.Reset();
        }
    }
}
