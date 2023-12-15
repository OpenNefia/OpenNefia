using OpenNefia.Content.CharaAppearance;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Core.Directions;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NetVips.Enums;

namespace OpenNefia.Content.PCCs
{
    public interface IPCCSystem : IEntitySystem
    {
        /// <summary>
        /// Initializes/removes the PCC of an entity.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="pccComp"></param>
        void SetupPCCDrawable(EntityUid uid, PCCComponent? pccComp = null);

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
            SubscribeComponent<PCCComponent, ComponentStartup>(OnComponentStartup);
            SubscribeComponent<PCCComponent, ComponentShutdown>(OnComponentShutdown);
            SubscribeComponent<PCCComponent, CharaAppearanceChangedEvent>(OnAppearanceChanged);
            SubscribeComponent<PCCComponent, EntityTurnStartingEventArgs>(OnTurnStarting);
            SubscribeComponent<PCCComponent, BeforeMoveEventArgs>(BeforeMove_UpdatePCC, priority: EventPriorities.VeryLow);
        }

        private void OnComponentStartup(EntityUid uid, PCCComponent pccComp, ComponentStartup args)
        {
            SetupPCCDrawable(uid, pccComp);
        }

        private void OnAppearanceChanged(EntityUid uid, PCCComponent pccComp, CharaAppearanceChangedEvent args)
        {
            SetupPCCDrawable(uid, pccComp);
        }

        public void SetupPCCDrawable(EntityUid uid, PCCComponent? pccComp = null)
        {
            if (!Resolve(uid, ref pccComp))
                return;

            if (pccComp.UsePCC)
            {
                var pccDrawable = PCCHelpers.CreatePCCDrawable(pccComp, _resourceCache);
                var entityDrawable = new EntityDrawableEntry(pccDrawable, hidesChip: true, zOrder: 1000);
                _drawables.RegisterDrawable(uid, DrawableID, entityDrawable);
            }
            else
            {
                _drawables.UnregisterDrawable(uid, DrawableID);
            }
        }

        private void OnComponentShutdown(EntityUid uid, PCCComponent pccComp, ComponentShutdown args)
        {
            _drawables.UnregisterDrawable(uid, DrawableID);
        }

        private void BeforeMove_UpdatePCC(EntityUid uid, PCCComponent pccComp, BeforeMoveEventArgs args)
        {
            if (args.Handled)
                return;

            UpdatePCC(uid, pccComp, Spatial(uid).Direction);
        }

        private void OnTurnStarting(EntityUid uid, PCCComponent pccComp, EntityTurnStartingEventArgs args)
        {
            if (!pccComp.UsePCC || !TryGetPCCDrawable(uid, out var pccDrawable, pccComp))
                return;

            UpdatePCC(uid, pccComp, Spatial(uid).Direction);
        }

        private void UpdatePCC(EntityUid uid, PCCComponent pccComp, MapCoordinates oldPosition, MapCoordinates newPosition)
        {
            if (!oldPosition.TryDirectionTowards(newPosition, out var dir))
                dir = Spatial(uid).Direction;
            UpdatePCC(uid, pccComp, dir);
        }

        private void UpdatePCC(EntityUid uid, PCCComponent pccComp, Core.Maths.Direction dir)
        {
            if (!pccComp.UsePCC || !TryGetPCCDrawable(uid, out var pccDrawable, pccComp))
                return;

            var frame = CompOrNull<TurnOrderComponent>(uid)?.TotalTurnsTaken ?? (pccDrawable.Frame + 1);

            pccComp.PCCDirection = dir.ToPCCDirection();
            pccDrawable.IsFullSize = pccComp.IsFullSize;
            pccDrawable.Direction = pccComp.PCCDirection;
            pccDrawable.Frame = frame % 4;
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
            if (!Resolve(uid, ref pccComp) || !TryGetPCCDrawable(uid, out var pccDrawable, pccComp))
                return;

            pccDrawable.IsFullSize = pccComp.IsFullSize;
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
