using OpenNefia.Content.Logic;
using OpenNefia.Content.Mount;
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

namespace OpenNefia.Content.Effects
{
    public sealed class TemporaryAllySystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeComponent<TemporaryAllyComponent, BeforeEntityMountedOntoEvent>(TempAlly_BlockRiding);
            SubscribeComponent<TemporaryAllyComponent, BeforeEntityStartsRidingEvent>(TempAlly_BlockRiding);
        }

        // >>>>>>>> elona122/shade2/proc.hsp:2233 	if (cBit(cBodyguard,tc)=true)or(cBit(cGuardTemp,t ...
        private void TempAlly_BlockRiding(EntityUid uid, TemporaryAllyComponent component, BeforeEntityMountedOntoEvent args)
        {
            if (args.Cancelled)
                return;

            _mes.Display(Loc.GetString("Elona.Mount.Start.Problems.CannotRideClient", ("rider", args.Rider), ("mount", uid)));
            args.Cancel();
        }

        private void TempAlly_BlockRiding(EntityUid uid, TemporaryAllyComponent component, BeforeEntityStartsRidingEvent args)
        {
            if (args.Cancelled)
                return;

            _mes.Display(Loc.GetString("Elona.Mount.Start.Problems.CannotRideClient", ("rider", uid), ("mount", args.Mount)));
            args.Cancel();
        }
        // <<<<<<<< elona122/shade2/proc.hsp:2236 		} ...
    }
}