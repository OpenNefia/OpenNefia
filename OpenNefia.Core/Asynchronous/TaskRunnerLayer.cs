using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Core.Asynchronous
{
    public class LoveTaskRunner : ITaskRunner
    {
        [Dependency] private readonly IUiLayerManager _layerManager = default!;

        public void Run(Task task)
        {
            var layer = new TaskRunnerLayer(task);
            _layerManager.Query(layer);
        }

        public T Run<T>(Task<T> task)
        {
            var layer = new TaskRunnerLayer(task);
            _layerManager.Query(layer);
            return task.Result;
        }
    }

    /// <summary>
    /// Layer for running an async task synchronously, by displaying a temporary
    /// loading screen until the task is finished.
    /// </summary>
    public class TaskRunnerLayer : BaseUiLayer<UiNoResult>
    {
        private Task ActiveTask;
        private float Dt;

        public TaskRunnerLayer(Task task)
        {
            ActiveTask = task;
        }

        public override void Update(float dt)
        {
            Dt += dt;

            if (ActiveTask.IsCompleted)
            {
                Finish(new UiNoResult());
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
