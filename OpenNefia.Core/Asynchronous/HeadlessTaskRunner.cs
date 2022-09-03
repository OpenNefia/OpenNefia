using OpenNefia.Core.GameController;
using OpenNefia.Core.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Asynchronous
{
    public class HeadlessTaskRunner : ITaskRunner
    {
        [Dependency] private readonly IGameController _gc = default!;

        public void Run(Task task)
        {
            while (!task.IsCompleted)
            {
                _gc.StepFrame();
            }
        }

        public T Run<T>(Task<T> task)
        {
            while (!task.IsCompleted)
            {
                _gc.StepFrame();
            }
            return task.Result;
        }
    }
}
