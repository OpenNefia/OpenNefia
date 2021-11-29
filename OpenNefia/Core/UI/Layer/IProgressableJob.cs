using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Layer
{
    public class ProgressOperation
    {
        public double Progress { get; private set; }
        public bool IsCompleted { get; private set; }

        public void Report(double progress)
        {
            if (progress < 0 || progress > 1)
                throw new ArgumentException("Progress must be between 0 and 1.", nameof(progress));

            // If completed - throw
            if (IsCompleted)
                throw new InvalidOperationException("Cannot report progress on an operation marked as completed.");

            // Set new progress
            Progress = progress;
        }
    }

    public delegate Task ProgressHandler(ProgressOperation op);

    public class ProgressStep
    {
        public string Text { get; }
        public ProgressHandler Delegate { get; }

        public ProgressStep(string text, ProgressHandler @delegate)
        {
            Text = text;
            Delegate = @delegate;
        }

        public ProgressStep(string text, Task task)
        {
            Text = text;
            Delegate = async (_) => await task;
        }
    }

    public interface IProgressableJob : IEnumerable<ProgressStep>
    {
        uint NumberOfSteps { get; }
    }
}