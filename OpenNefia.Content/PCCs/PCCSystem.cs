using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Utility;
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
        /// <summary>
        /// Gets the underlying PCC drawable for this entity, if it exists.
        /// </summary>
        bool TryGetPCCDrawable(EntityUid uid, [NotNullWhen(true)] out PCCDrawable? pccDrawable, 
            PCCComponent? pccComp = null);

        /// <summary>
        /// Updates the PCC image on this entity, if it exists. Call this whenever you update the list of
        /// <see cref="PCCComponent.PCCParts"/> on the entity's <see cref="PCCComponent"/>.
        /// </summary>
        void RebakePCCImage(EntityUid uid, PCCComponent? pccComp = null);

        /// <summary>
        /// Sets the PCC data for this entity, creating a <see cref="PCCComponent"/> on it if it's missing,
        /// and rebakes the PCC image.
        /// </summary>
        /// <param name="entity">Entity to set PCC data for.</param>
        /// <param name="parts">Set of PCC parts to use. This replaces any existing PCC parts.</param>
        void SetPCCParts(EntityUid entity, Dictionary<string, PCCPart> parts, PCCComponent? pccComp = null);
    }

    public sealed class PCCSystem : EntitySystem, IPCCSystem
    {
        public const string DrawableID = "Elona.PCC";

        [Dependency] private readonly IEntityDrawablesSystem _drawables = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<PCCComponent, ComponentStartup>(OnComponentStartup);
            SubscribeLocalEvent<PCCComponent, ComponentShutdown>(OnComponentShutdown);
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

        public void SetPCCParts(EntityUid uid, Dictionary<string, PCCPart> parts, PCCComponent? pccComp = null)
        {
            if (!Resolve(uid, ref pccComp))
                pccComp = EntityManager.EnsureComponent<PCCComponent>(uid);

            pccComp.PCCParts.Clear();
            pccComp.PCCParts.AddRange(parts);

            RebakePCCImage(uid, pccComp);
        }
    }
}
