using OpenNefia.Core.Audio;
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

    public class ProgressBarLayer : BaseUiLayer<ProgressBarLayer.Result>
    {
        public new class Result
        {
            public ProgressState State;

            public Result(ProgressState state)
            {
                State = state;
            }
        }

        public IProgressableJob Job { get; }
        private IEnumerator<ProgressStep> Steps;
        private bool HasNext = false;
        private int StepNumber = 0;

        private float ProgressPercent => Math.Clamp((float)this.StepNumber / (float)this.Job.NumberOfSteps, 0f, 1f);

        private FontSpec FontListText;
        private ColorDef ColorTextBlack;
        private IUiText TextStatus;
        private UiWindow Window;

        public ProgressBarLayer(IProgressableJob job)
        {
            this.Job = job;
            this.Steps = job.GetEnumerator();

            this.FontListText = FontDefOf.ListText;
            this.ColorTextBlack = ColorDefOf.TextBlack;
            this.TextStatus = new UiText(this.FontListText);
            this.Window = new UiWindow();

            if (this.AdvanceStep())
            {
                this.TextStatus.Text = this.Steps.Current.Text;
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
            this.TextStatus.SetPosition(this.Left + this.Width / 2 - this.TextStatus.Width / 2, this.Top + this.Height / 2 - this.TextStatus.Height * 3);
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
                this.Finish(new Result(ProgressState.Succeeded));
            }
            if (Steps.Current.Task.IsCompletedSuccessfully)
            {
                if (this.AdvanceStep())
                {
                    this.TextStatus.Text = this.Steps.Current.Text;
                    this.SetPosition(Left, Top);
                }
            }
            else if (Steps.Current.Task.IsCompleted)
            {
                this.Finish(new Result(ProgressState.Failed));
            }

            this.Window.Update(dt);
            this.TextStatus.Update(dt); 
        }

        public override void Draw()
        {
            this.Window.Draw();

            this.TextStatus.Draw();

            GraphicsEx.SetColor(this.ColorTextBlack);
            Love.Graphics.Rectangle(Love.DrawMode.Line, this.Left + 30, this.Top + this.Height / 2 - 10, this.Width - 60, 20);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, this.Left + 30, this.Top + this.Height / 2 - 10, (int)((this.Width - 60) * this.ProgressPercent), 20);
        }

        public override void Dispose()
        {
            this.Window.Dispose();
        }
    }
}
