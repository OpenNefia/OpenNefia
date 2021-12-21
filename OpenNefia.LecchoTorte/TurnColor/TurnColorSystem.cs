using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.LecchoTorte.TurnColor
{
    /// <summary>
    /// TODO: this needs to be in terms of temporary values instead.
    /// It's just a test for the slots system right now.
    /// </summary>
    public class TurnColorSystem : EntitySystem
    {
        public override void Initialize()
        {
            EntityManager.ComponentAdded += HandleComponentEvent;
            EntityManager.ComponentRemoved += HandleComponentEvent;
        }

        public override void Shutdown()
        {
            EntityManager.ComponentAdded -= HandleComponentEvent;
            EntityManager.ComponentRemoved -= HandleComponentEvent;
        }

        private void HandleComponentEvent(object? sender, ComponentEventArgs args)
        {
            if (args.Component is not TurnColorComponent turnColor)
                return;

            var uid = args.OwnerUid;
            ChipComponent? chip = null;

            if (!Resolve(uid, ref chip))
                return;

            chip.Color = turnColor.Color;
        }
    }
}
