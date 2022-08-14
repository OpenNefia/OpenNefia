using Love;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public interface IEntityDrawable : IDisposable
    {
        void Update(float dt);
        void Draw(float x, float y, float scaleX, float scaleY);
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

    public sealed class DrawablesSystem : EntitySystem, IEntityDrawablesSystem
    {
        public void RegisterDrawable(EntityUid entity, string key, EntityDrawableEntry drawable,
            EntityDrawablesComponent? drawables = null)
        {
            if (!Resolve(entity, ref drawables, logMissing: false))
            {
                drawables = EntityManager.EnsureComponent<EntityDrawablesComponent>(entity);
            }

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
