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
using OpenNefia.Content.GameObjects;

namespace OpenNefia.Content.Effects.New.EffectTargets
{
    public sealed class VanillaEffectTargetsSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly ITargetingSystem _targeting = default!;

        public override void Initialize()
        {
            SubscribeComponent<EffectTargetOtherComponent, GetEffectTargetEvent>(GetTarget_Other);
        }

        private void GetTarget_Other(EntityUid uid, EffectTargetOtherComponent component, GetEffectTargetEvent args)
        {
            if (args.Cancelled)
                return;

            // TODO
            if (_targeting.TryGetTarget(uid, out var target))
            {
                args.OutTarget = target.Value;
            }
            else
            {
                args.Cancel();
            }
        }
    }
}