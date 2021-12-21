﻿using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.Content.UI.Layer
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

        private float ProgressPercent => Math.Clamp(StepNumber / (float)Job.NumberOfSteps, 0f, 1f);

        private FontSpec FontListText = UiFonts.ListText;
        private Color ColorTextBlack = UiColors.TextBlack;
        private IUiText TextStatus;
        private UiWindow Window;

        public ProgressBarLayer(IProgressableJob job)
        {
            Job = job;
            Steps = job.GetEnumerator();

            TextStatus = new UiText(FontListText);
            Window = new UiWindow();

            if (AdvanceStep())
            {
                TextStatus.Text = Steps.Current.Text;
            }
        }

        private bool AdvanceStep()
        {
            var hasNext = Steps.MoveNext();
            StepNumber++;

            if (hasNext)
            {
                _currentOperation = new ProgressOperation();
                TextStatus.Text = Steps.Current.Text;
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
            Sounds.Play(Protos.Sound.Pop2);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Window.SetSize(width, height);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Window.SetPosition(x, y);
            TextStatus.SetPosition(X + Width / 2 - TextStatus.Width / 2, Y + Height / 2 - TextStatus.Height * 3);
        }

        public override void GetPreferredBounds(out UIBox2i bounds)
        {
            UiUtils.GetCenteredParams(400, 200, out bounds);
        }

        public override void Update(float dt)
        {
            if (_currentTask == null)
            {
                Finish(new UiNoResult());
            }
            else if (_currentTask.IsCompletedSuccessfully)
            {
                if (AdvanceStep())
                {
                    SetPosition(X, Y);
                }
            }
            else if (_currentTask.IsCompleted)
            {
                Logger.Log(LogLevel.Error, $"Progress task failed: {_currentTask.Exception}");
                Exception exception = _currentTask.Exception != null
                    ? _currentTask.Exception
                    : new InvalidOperationException("Progress task failed.");
                Error(exception);
            }

            Window.Update(dt);
            TextStatus.Update(dt);
        }

        public override void Draw()
        {
            Window.Draw();

            TextStatus.Draw();

            GraphicsEx.SetColor(ColorTextBlack);
            Love.Graphics.Rectangle(Love.DrawMode.Line, X + 30, Y + Height / 2 - 10, Width - 60, 20);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, X + 30, Y + Height / 2 - 10, (int)((Width - 60) * ProgressPercent), 20);
        }

        public override void Dispose()
        {
            Window.Dispose();
        }
    }
}
