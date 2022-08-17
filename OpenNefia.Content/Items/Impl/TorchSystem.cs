using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.MapVisibility;
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

namespace OpenNefia.Content.Items.Impl
{
    public sealed class TorchSystem : EntitySystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;

        public override void Initialize()
        {
            SubscribeComponent<TorchComponent, GetVerbsEventArgs>(GetVerbs_Torch);
        }

        private void GetVerbs_Torch(EntityUid uid, TorchComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new(UseInventoryBehavior.VerbTypeUse, "Use Torch", () => UseTorch(args.Source, args.Target)));
        }

        private TurnResult UseTorch(EntityUid source, EntityUid target)
        {
            if (!TryComp<DungeonLightComponent>(target, out var light))
                return TurnResult.Aborted;

            if (light.IsLit)
            {
                light.IsLit = false;
                _mes.Display(Loc.GetString("Elona.Items.Torch.PutOut", ("entity", source), ("item", target)), entity: source);
            }
            else
            {
                light.IsLit = true;
                _mes.Display(Loc.GetString("Elona.Items.Torch.Light", ("entity", source), ("item", target)), entity: source);
            }

            return TurnResult.Aborted;
        }
    }
}