using OpenNefia.Core.Game;
using OpenNefia.Core.GameController;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering
{
    public class MapDrawables : BaseDrawable, IMapDrawables
    {
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

        [Dependency] private readonly IGameController _gameController = default!;

        private SortedSet<Entry> Active = new();

        public void Enqueue(IMapDrawable drawable, MapCoordinates pos, int zOrder = 0)
        {
            if (pos.MapId != GameSession.ActiveMap?.Id)
                return;

            GameSession.Coords.TileToScreen(pos.Position, out var screenPos);
            drawable.ScreenLocalPos = screenPos;
            drawable.OnEnqueue();
            Active.Add(new Entry(drawable, zOrder));
        }

        public void Clear()
        {
            Active.Clear();
        }

        public bool HasActiveDrawables() => Active.Count > 0;

        /// <summary>
        /// Called from update code.
        /// </summary>
        public void WaitForDrawables()
        {
            while (HasActiveDrawables())
            {
                var dt = Love.Timer.GetDelta();
                _gameController.Update(dt);
                this.Update(dt);

                _gameController.Draw();
                _gameController.SystemStep();
            }
        }

        public override void Update(float dt)
        {
            foreach (var entry in Active)
            {
                var drawable = entry.Drawable;
                drawable.Update(dt);
                drawable.SetPosition(this.X + drawable.ScreenLocalPos.X, this.Y + drawable.ScreenLocalPos.Y);
            }

            Active.RemoveWhere(entry => entry.Drawable.IsFinished);
        }

        public override void Draw()
        {
            foreach (var entry in Active)
            {
                if (!entry.Drawable.IsFinished)
                {
                    entry.Drawable.Draw();
                }
            }
        }
    }
}
