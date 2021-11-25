using System.Threading;

namespace Why.Core.Timing
{
    public interface ITimerManager
    {
        void AddTimer(Timer timer, CancellationToken cancellationToken = default);

        void UpdateTimers(FrameEventArgs frameEventArgs);
    }
}
