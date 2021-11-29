using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;

namespace OpenNefia.Core.Util
{
    /// <summary>
    /// Layer for running an async task synchronously, by displaying a temporary
    /// loading screen until the task is finished.
    /// </summary>
    internal class TaskRunnerLayer : BaseUiLayer<UiNoResult>
    {
        private Task ActiveTask;
        private float Dt;

        public TaskRunnerLayer(Task task)
        {
            this.ActiveTask = task;
        }

        public override void Update(float dt)
        {
            Dt += dt;

            if (this.ActiveTask.IsCompleted)
            {
                this.Finish(new UiNoResult());
            }
        }

        public override void Draw()
        {
            GraphicsEx.SetColor(0, 0, 0, 128);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, this.Left, this.Top, this.Width, this.Height);

            {
                Love.Graphics.Push();
                Love.Graphics.Translate(this.Left + this.Width / 2, this.Top + this.Height / 2);
                Love.Graphics.Rotate(Dt);
                Love.Graphics.SetColor(Love.Color.White);
                Love.Graphics.Rectangle(Love.DrawMode.Fill, -32, -32, 64, 64);
                Love.Graphics.Pop();
            }
        }

        public static void Run(Task task)
        {
            new TaskRunnerLayer(task).Query();
        }

        public static T Run<T>(Task<T> task)
        {
            new TaskRunnerLayer(task).Query();
            return task.Result;
        }
    }
}
