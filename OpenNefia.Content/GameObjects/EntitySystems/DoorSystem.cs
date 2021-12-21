using OpenNefia.Content.Logic;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    public class DoorSystem : EntitySystem
    {
        [Dependency] private readonly IAudioSystem _sounds = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<DoorComponent, MapInitEvent>(HandleInitialize, nameof(HandleInitialize));
            SubscribeLocalEvent<DoorComponent, WasCollidedWithEventArgs>(HandleCollidedWith, nameof(HandleCollidedWith));
        }

        private void HandleInitialize(EntityUid uid, DoorComponent door, ref MapInitEvent args)
        {
            if (EntityManager.TryGetComponent<SpatialComponent>(uid, out var spatial))
            {
                if (door.IsOpen)
                {
                    spatial.IsSolid = false;
                    spatial.IsOpaque = false;
                }
                else
                {
                    spatial.IsSolid = true;
                    spatial.IsOpaque = true;
                }
            }

            if (EntityManager.TryGetComponent<ChipComponent>(uid, out var chip))
            {
                if (door.IsOpen)
                {
                    chip.ChipID = door.ChipOpen;
                }
                else
                {
                    chip.ChipID = door.ChipClosed;
                }
            }
        }

        private void HandleCollidedWith(EntityUid uid, DoorComponent door, WasCollidedWithEventArgs args)
        {
            Mes.Display($"{DisplayNameSystem.GetDisplayName(args.Source)} opens {DisplayNameSystem.GetDisplayName(uid)}");

            if (door.SoundOpen != null)
            {
                _sounds.Play(door.SoundOpen.Value, uid);
            }

            door.IsOpen = true;
            args.Handle(TurnResult.Succeeded);
        }
    }
}
