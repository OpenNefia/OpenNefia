using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Layer
{
    public class ProgressStep
    {
        public string Text;
        public Task Task;

        public ProgressStep(string text, Task task)
        {
            Text = text;
            Task = task;
        }
    }

    public interface IProgressableJob : IEnumerable<ProgressStep>
    {
        uint NumberOfSteps { get; }
    }
}