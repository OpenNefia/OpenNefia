using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Layer
{
    /// <summary>
    /// This progress bar can be shown before defs are loaded.
    /// </summary>
    public class MinimalProgressBarLayer : BaseUiLayer<UiNoResult>
    {
        public IProgressableJob Job { get; }
        private IEnumerator<ProgressStep> Steps;

        private Love.Text LoadingText;
        private Love.Text StatusText;

        private ProgressOperation _currentOperation = new();
        private Task? _currentTask;

        private int StepNumber = 0;

        private float ProgressPercent => Math.Clamp((float)this.StepNumber / (float)this.Job.NumberOfSteps, 0f, 1f);

        private FontSpec FontTextLarge = new(20, 20);
        private FontSpec FontTextSmall = new(14, 14);

        public MinimalProgressBarLayer(IProgressableJob job)
        {
            this.Job = job;
            this.Steps = job.GetEnumerator();
            this.LoadingText = Love.Graphics.NewText(FontTextLarge.LoveFont, "Now Loading...");
            this.StatusText = Love.Graphics.NewText(FontTextSmall.LoveFont, string.Empty);

            this.AdvanceStep();
            
        }

        private bool AdvanceStep()
        {
            var hasNext = this.Steps.MoveNext();
            this.StepNumber++;

            if (hasNext)
            {
                _currentOperation = new ProgressOperation();
                this.StatusText.Set(Love.ColoredStringArray.Create(this.Steps.Current.Text));
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
        }

        public override void GetPreferredBounds(out UIBox2i bounds)
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
        }

        public override void Draw()
        {
            Love.Graphics.Clear(Love.Color.Black);

            var x = this.X + this.Width / 2;
            var y = this.Y + this.Height / 2;
            var barWidth = 400;
            var barHeight = 20;

            Love.Graphics.SetColor(Love.Color.White);
            Love.Graphics.Draw(this.LoadingText, x - this.LoadingText.GetWidth() / 2, y - this.LoadingText.GetHeight() / 2 - 24 - 4 - barHeight);

            Love.Graphics.Draw(this.StatusText, x - this.StatusText.GetWidth() / 2, y - this.StatusText.GetHeight() / 2 - 4 - barHeight);

            Love.Graphics.Rectangle(Love.DrawMode.Line, x - barWidth / 2, y, barWidth, barHeight);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, x - barWidth / 2, y, (int)(barWidth * this.ProgressPercent), barHeight);

            Love.Graphics.Rectangle(Love.DrawMode.Line, x - barWidth / 2, y + barHeight + 4, barWidth, barHeight);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, x - barWidth / 2, y + barHeight + 4, (int)(barWidth * this._currentOperation.Progress), barHeight);
        }

        public override void Dispose()
        {
            this.LoadingText.Dispose();
            this.StatusText.Dispose();
        }
    }
}
