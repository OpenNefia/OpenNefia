using Love;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;

namespace OpenNefia.Core.Rendering
{
    /// <summary>
    /// Replaces the default batched tile rendering for an entity with
    /// custom rendering logic. This causes a slight performance loss
    /// as it means all entities can no longer be drawn with a single
    /// batched draw call inside <see cref="ChipBatch"/>.
    /// 
    /// The drawable is assumed to be the same size as a tile in the
    /// current coordinate system (48 pixels by default).
    /// </summary>
    [ImplicitDataDefinitionForInheritors]
    public interface IEntityDrawable : IDisposable
    {
        void Initialize(IResourceCache cache);
        void Update(float dt);
        void Draw(float scale, float x, float y, float scaleX = 1f, float scaleY = 1f);
    }

    public sealed class EntityDrawableEntry
    {
        public IEntityDrawable Drawable { get; set; }
        public bool HidesChip { get; set; }
        public int ZOrder { get; set; } = 0;

        public EntityDrawableEntry(IEntityDrawable drawable, bool hidesChip, int zOrder = 0)
        {
            Drawable = drawable;
            HidesChip = hidesChip;
            ZOrder = zOrder;
        }
    }

    public interface IEntityDrawablesSystem : IEntitySystem
    {
        void RegisterDrawable(EntityUid entity, string key, EntityDrawableEntry drawable, 
            EntityDrawablesComponent? drawables = null);

        void UnregisterDrawable(EntityUid entity, string key, EntityDrawablesComponent? drawables = null);

        bool TryGetDrawable(EntityUid entity, string key, [NotNullWhen(true)] out EntityDrawableEntry? drawable,
            EntityDrawablesComponent? drawables = null);

        void ClearDrawables(EntityUid entity, EntityDrawablesComponent? drawables = null);
    }

    public sealed class EntityDrawablesSystem : EntitySystem, IEntityDrawablesSystem
    {
        [Dependency] private readonly IResourceCache _resourceCache = default!;

        public override void Initialize()
        {
            SubscribeComponent<EntityDrawablesComponent, GetMapObjectMemoryEventArgs>(AttachDrawables);
        }

        private void AttachDrawables(EntityUid uid, EntityDrawablesComponent component, GetMapObjectMemoryEventArgs args)
        {
            foreach (var drawable in component.EntityDrawables.Values)
            {
                args.OutMemory.Drawables.Add(drawable.Drawable);
                if (drawable.HidesChip)
                    args.OutMemory.AtlasIndex = null;
            }
        }

        public void RegisterDrawable(EntityUid entity, string key, EntityDrawableEntry drawable,
            EntityDrawablesComponent? drawables = null)
        {
            if (!Resolve(entity, ref drawables, logMissing: false))
            {
                drawables = EntityManager.EnsureComponent<EntityDrawablesComponent>(entity);
            }

            drawable.Drawable.Initialize(_resourceCache);
            drawables.EntityDrawables.Add(key, drawable);
        }

        public void UnregisterDrawable(EntityUid entity, string key, EntityDrawablesComponent? drawables = null)
        {
            if (!Resolve(entity, ref drawables, logMissing: false))
                return;

            drawables.EntityDrawables.Remove(key);
        }

        public bool TryGetDrawable(EntityUid entity, string key,
            [NotNullWhen(true)] out EntityDrawableEntry? drawable, 
            EntityDrawablesComponent? drawables = null)
        {
            if (!Resolve(entity, ref drawables, logMissing: false))
            {
                drawable = null;
                return false;
            }

            return drawables.EntityDrawables.TryGetValue(key, out drawable);
        }

        public void ClearDrawables(EntityUid entity, EntityDrawablesComponent? drawables = null)
        {
            if (!Resolve(entity, ref drawables, logMissing: false))
                return;

            drawables.EntityDrawables.Clear();
        }
    }
}
