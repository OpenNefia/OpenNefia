using Love;
using OpenNefia.Core.Game;
using OpenNefia.Core.Map;
using OpenNefia.Core.Maps;
using OpenNefia.Core.UI.Element;
using OpenNefia.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNefia.Core.Rendering
{
    public class MapDrawables : BaseDrawable
    {
        private class MapDrawableEntry : IComparable<MapDrawableEntry>
        {
            public IMapDrawable Drawable;
            public int ZOrder;

            public MapDrawableEntry(IMapDrawable drawable, int zOrder = 0)
            {
                Drawable = drawable;
                ZOrder = zOrder;
            }

            public int CompareTo(MapDrawableEntry? other)
            {
                if (ZOrder == other?.ZOrder)
                {
                    return Drawable == other.Drawable ? 0 : -1;
                }
                return ZOrder.CompareTo(other?.ZOrder);
            }
        }

        private SortedSet<MapDrawableEntry> Active = new SortedSet<MapDrawableEntry>();

        public void Enqueue(IMapDrawable drawable, MapCoordinates? pos, int zOrder = 0)
        {
            if (pos == null || pos.Value.MapId != GameSession.ActiveMap.Id)
                return;

            GameSession.Coords.TileToScreen(pos.Value.X, pos.Value.Y, out var screenX, out var screenY);
            drawable.ScreenLocalX = screenX;
            drawable.ScreenLocalY = screenY;
            drawable.OnEnqueue();
            Active.Add(new MapDrawableEntry(drawable, zOrder));
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
                var dt = Timer.GetDelta();
                Engine.Instance.Update(dt);
                this.Update(dt);

                Engine.Instance.Draw();
                Engine.Instance.SystemStep();
            }
        }

        public override void Update(float dt)
        {
            foreach (var entry in Active)
            {
                var drawable = entry.Drawable;
                drawable.Update(dt);
                drawable.SetPosition(this.X + drawable.ScreenLocalX, this.Y + drawable.ScreenLocalY);
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
