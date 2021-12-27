using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Asynchronous
{
    public class HeadlessTaskRunner : ITaskRunner
    {
        public void Run(Task task)
        {
            while (!task.IsCompleted) {}
        }

        public T Run<T>(Task<T> task)
        {
            while (!task.IsCompleted) {}
            return task.Result;
        }
    }
}
