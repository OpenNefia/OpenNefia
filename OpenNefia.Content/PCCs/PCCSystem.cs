using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.ResourceManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.PCCs
{
    public interface IPCCSystem : IEntitySystem
    {
        bool TryGetPCCDrawable(EntityUid uid, [NotNullWhen(true)] out PCCDrawable? pccDrawable, 
            PCCComponent? pccComp = null);

        void RebakePCCImage(EntityUid uid, PCCComponent? pccComp = null);
    }

    public sealed class PCCSystem : EntitySystem, IPCCSystem
    {
        public const string DrawableID = "Elona.PCC";

        [Dependency] private readonly IDrawablesSystem _drawables = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<PCCComponent, ComponentStartup>(OnComponentStartup, nameof(OnComponentStartup));
            SubscribeLocalEvent<PCCComponent, ComponentShutdown>(OnComponentShutdown, nameof(OnComponentShutdown));
        }

        private void OnComponentStartup(EntityUid uid, PCCComponent pccComp, ComponentStartup args)
        {
            var pccDrawable = new PCCDrawable(pccComp.PCCParts);
            pccDrawable.RebakeImage(_resourceCache);

            var entityDrawable = new EntityDrawableEntry(pccDrawable, true);
            _drawables.RegisterDrawable(uid, DrawableID, entityDrawable);
        }

        private void OnComponentShutdown(EntityUid uid, PCCComponent pccComp, ComponentShutdown args)
        {
            _drawables.UnregisterDrawable(uid, DrawableID);
        }

        public bool TryGetPCCDrawable(EntityUid uid, [NotNullWhen(true)] out PCCDrawable? pccDrawable,
            PCCComponent? pccComp = null)
        {
            pccDrawable = null;

            if (!Resolve(uid, ref pccComp))
                return false;

            if (!_drawables.TryGetDrawable(uid, DrawableID, out var entDrawable))
                return false;

            pccDrawable = entDrawable.Drawable as PCCDrawable;
            return pccDrawable != null;
        }

        public void RebakePCCImage(EntityUid uid, PCCComponent? pccComp = null)
        {
            if (!TryGetPCCDrawable(uid, out var pccDrawable, pccComp))
                return;

            pccDrawable.RebakeImage(_resourceCache);
        }
    }
}
