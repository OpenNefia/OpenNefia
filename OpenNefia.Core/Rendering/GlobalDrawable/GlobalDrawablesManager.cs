using OpenNefia.Core.GameController;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Timing;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NetVips.Enums;

namespace OpenNefia.Core.Rendering
{    
    /// <summary>
    /// Displays and updates animatable graphics in screenspace.
    /// </summary>
    public interface IGlobalDrawablesManager : IDrawable
    {
        void Clear();
        void Enqueue(IGlobalDrawable drawable, Vector2 screenPos, int zOrder = 0);
        bool HasActiveDrawables();
        void WaitForDrawables();
    }

    public sealed class GlobalDrawablesManager : BaseDrawable, IGlobalDrawablesManager
    {
        [Dependency] private readonly IGameController _gameController = default!;

        private class Entry : IComparable<Entry>
        {
            public IGlobalDrawable Drawable;
            public int ZOrder;

            public Entry(IGlobalDrawable drawable, int zOrder = 0)
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

        public void Enqueue(IGlobalDrawable drawable, Vector2 screenPos, int zOrder = 0)
{
            // TODO this needs to handle resizing, hence an anchoring system
            // like with IHud is necessary
            drawable.SetPosition(screenPos.X, screenPos.Y);
            drawable.OnEnqueue();
            _active.Add(new Entry(drawable, zOrder));

            // TODO configure
            WaitForDrawables();
        }

        public void Clear()
        {
            _active.Clear();
        }

        public bool HasActiveDrawables() => _active.Count > 0;

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
