using OpenNefia.Core.Game;
using OpenNefia.Core.GameController;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Timing;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering
{
    /// <summary>
    /// Displays and updates animatable graphics in worldspace.
    /// </summary>
    public interface IMapDrawablesManager : IDrawable
    {
        void Clear();
        void Enqueue(IMapDrawable drawable, MapCoordinates pos, int zOrder = 0);
        void Enqueue(IMapDrawable drawable, EntityUid ent, int zOrder = 0);
        bool HasActiveDrawables();
        void WaitForDrawables();
    }
    
    public class MapDrawablesManager : BaseDrawable, IMapDrawablesManager
    {
        [Dependency] private readonly IEntityManager _entityMan = default!;
        [Dependency] private readonly IMapManager _mapMan = default!;
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IGameController _gameController = default!;

        private class Entry : IComparable<Entry>
        {
            public IMapDrawable Drawable;
            public int ZOrder;

            public Entry(IMapDrawable drawable, int zOrder = 0)
            {
                Drawable = drawable;
                ZOrder = zOrder;
            }

            public int CompareTo(Entry? other)
            {
                if (ZOrder == other?.ZOrder)
                {
                    return Drawable == other.Drawable ? 0 : -1;
                }
                return ZOrder.CompareTo(other?.ZOrder);
            }
        }

        private SortedSet<Entry> _active = new();

        public void Enqueue(IMapDrawable drawable, MapCoordinates pos, int zOrder = 0)
        {
            if (pos.MapId != _mapMan.ActiveMap?.Id)
                return;

            var screenPos = _coords.TileToScreen(pos.Position);
            drawable.ScreenLocalPos = screenPos;
            drawable.OnEnqueue();
            _active.Add(new Entry(drawable, zOrder));
        }

        public void Enqueue(IMapDrawable drawable, EntityUid ent, int zOrder = 0)
        {
            if (!_entityMan.TryGetComponent<SpatialComponent>(ent, out var spatial))
                return;

            Enqueue(drawable, spatial.MapPosition, zOrder);
        }

        public void Clear()
        {
            _active.Clear();
        }

        public bool HasActiveDrawables() => _active.Count > 0;

        /// <summary>
        /// Called from update code.
        /// </summary>
        public void WaitForDrawables()
        {
            while (HasActiveDrawables())
            {
                var dt = Love.Timer.GetDelta();
                var frameArgs = new FrameEventArgs(dt, stepInput: false);
                _gameController.Update(frameArgs);
                Update(dt);

                _gameController.Draw();
                _gameController.SystemStep(stepInput: false);
            }
        }

        public override void Update(float dt)
        {
            foreach (var entry in _active)
            {
                var drawable = entry.Drawable;
                drawable.Update(dt);
                drawable.ScreenOffset = this.Position;
                drawable.SetPosition((drawable.ScreenOffset.X + drawable.ScreenLocalPos.X) * _coords.TileScale, (drawable.ScreenOffset.Y + drawable.ScreenLocalPos.Y) * _coords.TileScale);
            }

            _active.RemoveWhere(entry => entry.Drawable.IsFinished);
        }

        public override void Draw()
        {
            foreach (var entry in _active)
            {
                if (!entry.Drawable.IsFinished)
                {
                    entry.Drawable.Draw();
                }
            }
        }
    }
}
