using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Logic;
using OpenNefia.Core.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    public class DoorSystem : EntitySystem
    {
        public const string VerbIDClose = "Elona.Close";

        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<DoorComponent, GetVerbsEventArgs>(HandleGetVerbs, nameof(HandleGetVerbs));
            SubscribeLocalEvent<ExecuteVerbEventArgs>(HandleExecuteVerb, nameof(HandleExecuteVerb));
            SubscribeLocalEvent<DoorComponent, DoCloseEventArgs>(HandleClose, nameof(HandleClose));
            SubscribeLocalEvent<DoorComponent, EntityMapInitEvent>(HandleInitialize, nameof(HandleInitialize));
            SubscribeLocalEvent<DoorComponent, WasCollidedWithEventArgs>(HandleCollidedWith, nameof(HandleCollidedWith));
        }

        private void HandleGetVerbs(EntityUid uid, DoorComponent component, GetVerbsEventArgs args)
        {
            if (component.IsOpen)
            {
                args.Verbs.Add(new Verb(VerbIDClose));
            }
        }

        private void HandleExecuteVerb(ExecuteVerbEventArgs args)
        {
            if (args.Handled)
                return;

            switch (args.Verb.ID)
            {
                case VerbIDClose:
                    Raise(args.Target, new DoCloseEventArgs(args.Source), args);
                    break;
            }
        }

        private void HandleClose(EntityUid uid, DoorComponent door, DoCloseEventArgs args)
        {
            if (args.Handled)
                return;

            var result = DoClose(uid, door, args.Closer);

            if (result != null)
                args.Handle(result.Value);
        }

        private TurnResult? DoClose(EntityUid uid, DoorComponent door, EntityUid closer)
        {
            if (!door.IsOpen)
                return null;

            if (!EntityManager.TryGetComponent(uid, out SpatialComponent spatial))
                return null;

            if (!_mapManager.TryGetMap(spatial.MapID, out var map))
                return null;

            if (!map.CanAccess(spatial.MapPosition))
            {
                _mes.Display(Loc.GetString("Elona.Door.Close.Blocked"));
                return TurnResult.Aborted;
            }

            _mes.Display(Loc.GetString("Elona.Door.Close.Succeeds", ("entity", closer)));
            SetOpen(uid, false, door);

            return TurnResult.Succeeded;
        }

        public void SetOpen(EntityUid uid, bool isOpen, DoorComponent? door = null)
        {
            if (!Resolve(uid, ref door))
                return;

            door.IsOpen = isOpen;

            if (EntityManager.TryGetComponent(uid, out SpatialComponent spatial))
            {
                spatial.IsSolid = !isOpen;
                spatial.IsOpaque = !isOpen;
            }

            if (EntityManager.TryGetComponent(uid, out ChipComponent chip))
            {
                chip.ChipID = isOpen ? door.ChipOpen : door.ChipClosed;
            }
        }

        private void HandleInitialize(EntityUid uid, DoorComponent door, ref EntityMapInitEvent args)
        {
            SetOpen(uid, door.IsOpen, door);
        }

        private void HandleCollidedWith(EntityUid uid, DoorComponent door, WasCollidedWithEventArgs args)
        {
            _mes.Display(Loc.GetString("Elona.Door.Open.Succeeds", ("entity", args.Source)));

            if (door.SoundOpen != null)
            {
                var sound = door.SoundOpen.GetSound();
                if (sound != null)
                    _sounds.Play(sound.Value, uid);
            }

            SetOpen(uid, true, door);
            args.Handle(TurnResult.Succeeded);
        }
    }

    public class DoCloseEventArgs : TurnResultEntityEventArgs
    {
        public readonly EntityUid Closer;

        public DoCloseEventArgs(EntityUid closer)
        {
            Closer = closer;
        }
    }
}
