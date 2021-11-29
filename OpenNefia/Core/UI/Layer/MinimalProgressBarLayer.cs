using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Layer
{
    /// <summary>
    /// This progress bar can be shown before defs are loaded.
    /// </summary>
    public class MinimalProgressBarLayer : BaseUiLayer<ProgressBarLayer.Result>
    {
        public IProgressableJob Job { get; }
        private IEnumerator<ProgressStep> Steps;

        private Love.Text LoadingText;
        private Love.Text StatusText;

        private bool HasNext = false;
        private int StepNumber = 0;

        private float ProgressPercent => Math.Clamp((float)this.StepNumber / (float)this.Job.NumberOfSteps, 0f, 1f);

        public MinimalProgressBarLayer(IProgressableJob job)
        {
            this.Job = job;
            this.Steps = job.GetEnumerator();
            this.LoadingText = Love.Graphics.NewText(GraphicsEx.GetFont(20), "Now Loading...");
            this.StatusText = Love.Graphics.NewText(GraphicsEx.GetFont(14), string.Empty);

            if (this.AdvanceStep())
            {
                this.StatusText.Set(Love.ColoredStringArray.Create(this.Steps.Current.Text));
            }
        }

        private bool AdvanceStep()
        {
            this.HasNext = this.Steps.MoveNext();
            this.StepNumber++;
            return this.HasNext;
        }

        public override void OnQuery()
        {
        }

        public override void GetPreferredBounds(out int x, out int y, out int width, out int height)
        {
            var rect = UiUtils.GetCenteredParams(400, 200);
            x = rect.X;
            y = rect.Y;
            width = rect.Width;
            height = rect.Height;
        }

        public override void Update(float dt)
        {
            if (!HasNext)
            {
                this.Finish(new ProgressBarLayer.Result(ProgressState.Succeeded));
            }
            if (Steps.Current.Task.IsCompletedSuccessfully)
            {
                if (this.AdvanceStep())
                {
                    this.StatusText.Set(Love.ColoredStringArray.Create(this.Steps.Current.Text));
                    this.SetPosition(X, Y);
                }
            }
            else if (Steps.Current.Task.IsCompleted)
            {
                this.Finish(new ProgressBarLayer.Result(ProgressState.Failed));
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
        }

        public override void Dispose()
        {
            this.LoadingText.Dispose();
            this.StatusText.Dispose();
        }
    }
}
