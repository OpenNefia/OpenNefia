using System.Threading;

namespace OpenNefia.Core.Timing
{
    public interface ITimerManager
    {
        void AddTimer(Timer timer, CancellationToken cancellationToken = default);

        void UpdateTimers(FrameEventArgs frameEventArgs);
    }
}
