using OpenNefia.Content.Chests;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core;
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
using OpenNefia.Core.Audio;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Content.Items.Impl
{
    public interface ITrunkSystem : IEntitySystem
    {
    }

    public sealed class TrunkSystem : EntitySystem, ITrunkSystem
    {
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

        public override void Initialize()
        {
            SubscribeComponent<TrunkComponent, GetVerbsEventArgs>(GetVerbs_Trunk);
        }

        private void GetVerbs_Trunk(EntityUid uid, TrunkComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(OpenInventoryBehavior.VerbTypeOpen, "Open Chest", () => OpenTrunk(args.Source, args.Target, component)));
        }

        private TurnResult OpenTrunk(EntityUid source, EntityUid target, TrunkComponent trunk)
        {
            _audio.Play(Protos.Sound.Chest1, source);
            var context = new InventoryContext(source, target, new TakeInventoryBehavior(trunk.Container));
            var result = _uiManager.Query<InventoryLayer, InventoryContext, InventoryLayer.Result>(context);
            if (result.HasValue)
            {
                switch (result.Value.Data)
                {
                    case InventoryResult.Finished finished:
                        return finished.TurnResult;
                    default:
                        return TurnResult.Aborted;
                }
            }
            return TurnResult.Aborted;
        }
    }
}