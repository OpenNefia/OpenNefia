using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Timing;

namespace OpenNefia.Core.UI.Wisp
{
    /// <summary>
    /// TODO: Intent is for this to be merged into <see cref="UserInterface.IUserInterfaceManagerInternal"/>.
    /// </summary>
    public interface IWispManager
    {
        void QueueArrangeUpdate(WispControl control);
        void QueueMeasureUpdate(WispControl control);
        void Update(FrameEventArgs args);
    }

    public sealed class WispManager : IWispManager
    {
        [Dependency] private readonly IGraphics _graphics = default!;

        // private readonly Queue<WispControl> _styleUpdateQueue = new();
        private readonly Queue<WispControl> _measureUpdateQueue = new();
        private readonly Queue<WispControl> _arrangeUpdateQueue = new();

        public void QueueMeasureUpdate(WispControl control)
        {
            _measureUpdateQueue.Enqueue(control);
            _arrangeUpdateQueue.Enqueue(control);
        }

        public void QueueArrangeUpdate(WispControl control)
        {
            _arrangeUpdateQueue.Enqueue(control);
        }

        public void Update(FrameEventArgs args)
        {
            // Process queued style & layout updates.
            //while (_styleUpdateQueue.Count != 0)
            //{
            //    var control = _styleUpdateQueue.Dequeue();

            //    if (control.Disposed)
            //    {
            //        continue;
            //    }

            //    control.DoStyleUpdate();
            //}

            while (_measureUpdateQueue.Count != 0)
            {
                var control = _measureUpdateQueue.Dequeue();

                if (control.Disposed)
                {
                    continue;
                }

                RunMeasure(control);
            }

            while (_arrangeUpdateQueue.Count != 0)
            {
                var control = _arrangeUpdateQueue.Dequeue();

                if (control.Disposed)
                {
                    continue;
                }

                RunArrange(control);
            }

            // count down tooltip delay if we're not showing one yet and
            // are hovering the mouse over a control without moving it
            //if (_tooltipDelay != null && !showingTooltip)
            //{
            //    _tooltipTimer += args.DeltaSeconds;
            //    if (_tooltipTimer >= _tooltipDelay)
            //    {
            //        _showTooltip();
            //    }
            //}
        }

        private void RunMeasure(WispControl control)
        {
            if (control.IsMeasureValid || !control.IsInsideTree)
                return;

            if (control.Parent is WispControl wispParent)
            {
                RunMeasure(wispParent);
            }

            if (control is WispRoot root)
            {
                control.Measure(_graphics.WindowSize);
            }
            else if (control.PreviousMeasure.HasValue)
            {
                control.Measure(control.PreviousMeasure.Value);
            }
        }

        private void RunArrange(WispControl control)
        {
            if (control.IsArrangeValid || !control.IsInsideTree)
                return;

            if (control.Parent is WispControl wispParent)
            {
                RunArrange(wispParent);
            }

            if (control is WispRoot root)
            {
                control.Arrange(UIBox2.FromDimensions(Vector2.Zero, _graphics.WindowSize));
            }
            else if (control.PreviousArrange.HasValue)
            {
                control.Arrange(control.PreviousArrange.Value);
            }
        }
    }
}
