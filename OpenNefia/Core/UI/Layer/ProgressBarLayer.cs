using OpenNefia.Core.Audio;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Layer
{
    public enum ProgressState
    {
        Succeeded,
        Failed
    }

    public class ProgressBarLayer : BaseUiLayer<UiNoResult>
    {
        public IProgressableJob Job { get; }
        private IEnumerator<ProgressStep> Steps;
        private int StepNumber = 0;

        private ProgressOperation _currentOperation = new();
        private Task? _currentTask;

        private float ProgressPercent => Math.Clamp((float)this.StepNumber / (float)this.Job.NumberOfSteps, 0f, 1f);

        private FontSpec FontListText = UiFonts.ListText;
        private Color ColorTextBlack = UiColors.TextBlack;
        private IUiText TextStatus;
        private UiWindow Window;

        public ProgressBarLayer(IProgressableJob job)
        {
            this.Job = job;
            this.Steps = job.GetEnumerator();

            this.TextStatus = new UiText(this.FontListText);
            this.Window = new UiWindow();

            if (this.AdvanceStep())
            {
                this.TextStatus.Text = this.Steps.Current.Text;
            }
        }

        private bool AdvanceStep()
        {
            var hasNext = this.Steps.MoveNext();
            this.StepNumber++;

            if (hasNext)
            {
                _currentOperation = new ProgressOperation();
                this.TextStatus.Text = this.Steps.Current.Text;
                _currentTask = Steps.Current.Delegate(_currentOperation);
            }
            else
            {
                _currentTask = null;
            }

            return hasNext;
        }

        public override void OnQuery()
        {
            Sounds.Play(SoundPrototypeOf.Pop2);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            this.Window.SetSize(width, height);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            this.Window.SetPosition(x, y);
            this.TextStatus.SetPosition(this.X + this.Width / 2 - this.TextStatus.Width / 2, this.Y + this.Height / 2 - this.TextStatus.Height * 3);
        }

        public override void GetPreferredBounds(out Box2i bounds)
        {
            UiUtils.GetCenteredParams(400, 200, out bounds);
        }

        public override void Update(float dt)
        {
            if (_currentTask == null)
            {
                this.Finish(new UiNoResult());
            }
            else if (_currentTask.IsCompletedSuccessfully)
            {
                if (this.AdvanceStep())
                {
                    this.SetPosition(this.X, this.Y);
                }
            }
            else if (_currentTask.IsCompleted)
            {
                Logger.Log(LogLevel.Error, $"Progress task failed: {_currentTask.Exception}");
                Exception exception = _currentTask.Exception != null
                    ? _currentTask.Exception
                    : new InvalidOperationException("Progress task failed.");
                this.Error(exception);
            }

            this.Window.Update(dt);
            this.TextStatus.Update(dt); 
        }

        public override void Draw()
        {
            this.Window.Draw();

            this.TextStatus.Draw();

            GraphicsEx.SetColor(this.ColorTextBlack);
            Love.Graphics.Rectangle(Love.DrawMode.Line, this.X + 30, this.Y + this.Height / 2 - 10, this.Width - 60, 20);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, this.X + 30, this.Y + this.Height / 2 - 10, (int)((this.Width - 60) * this.ProgressPercent), 20);
        }

        public override void Dispose()
        {
            this.Window.Dispose();
        }
    }
}
