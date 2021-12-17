using OpenNefia.Content.Logic;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    public class DoorSystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<DoorComponent, MapInitEvent>(HandleInitialize, nameof(HandleInitialize));
            SubscribeLocalEvent<DoorComponent, WasCollidedWithEventArgs>(HandleCollidedWith, nameof(HandleCollidedWith));
        }

        private void HandleInitialize(EntityUid uid, DoorComponent door, ref MapInitEvent args)
        {
            ChipComponent? chip = null;
            SpatialComponent? spatial = null;

            if (!Resolve(uid, ref chip, ref spatial))
                return;

            if (door.IsOpen)
            {
                spatial.IsSolid = false;
                spatial.IsOpaque = false;
                chip.ChipID = door.ChipOpen;
            }
            else
            {
                spatial.IsSolid = true;
                spatial.IsOpaque = true;
                chip.ChipID = door.ChipClosed;
            }
        }

        private void HandleCollidedWith(EntityUid uid, DoorComponent door, WasCollidedWithEventArgs args)
        {
            Mes.Display($"{DisplayNameSystem.GetDisplayName(args.Source)} opens {DisplayNameSystem.GetDisplayName(uid)}");

            if (door.SoundOpen != null)
            {
                Sounds.Play(door.SoundOpen.Value, uid);
            }

            door.IsOpen = true;
            args.Handle(TurnResult.Succeeded);
        }
    }
}
