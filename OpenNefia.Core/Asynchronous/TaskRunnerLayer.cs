using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Core.Asynchronous
{
    public class LoveTaskRunner : ITaskRunner
    {
        [Dependency] private readonly IUserInterfaceManager _uiMgr = default!;

        public void Run(Task task)
        {
            _uiMgr.Query<TaskRunnerLayer, Task>(task);
        }

        public T Run<T>(Task<T> task)
        {
            _uiMgr.Query<TaskRunnerLayer, Task>(task);
            return task.Result;
        }
    }

    /// <summary>
    /// Layer for running an async task synchronously, by displaying a temporary
    /// loading screen until the task is finished.
    /// </summary>
    public class TaskRunnerLayer : UiLayerWithResult<Task, UINone>
    {
        private Task ActiveTask = default!;
        private float Dt;

        public override void Initialize(Task task)
        {
            ActiveTask = task;
        }

        public override void Update(float dt)
        {
            Dt += dt;

            if (ActiveTask.IsCompleted)
            {
                Finish(new UINone());
            }
        }

        public override void Draw()
        {
            GraphicsEx.SetColor(0, 0, 0, 128);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, X, Y, Width, Height);

            {
                Love.Graphics.Push();
                Love.Graphics.Translate(X + Width / 2, Y + Height / 2);
                Love.Graphics.Rotate(Dt);
                Love.Graphics.SetColor(Love.Color.White);
                Love.Graphics.Rectangle(Love.DrawMode.Fill, -32, -32, 64, 64);
                Love.Graphics.Pop();
            }
        }
    }
}
