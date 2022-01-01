using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
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
    public class MinimalProgressBarLayer : UiLayerWithResult<IProgressableJob, UINone>
    {
        public IProgressableJob Job { get; private set; } = default!;
        private IEnumerator<ProgressStep> Steps = default!;

        private Love.Text LoadingText;
        private Love.Text StatusText;

        private ProgressOperation _currentOperation = new();
        private Task? _currentTask;

        private int StepNumber = 0;

        private float ProgressPercent => Math.Clamp(StepNumber / (float)Job.NumberOfSteps, 0f, 1f);

        private FontSpec FontTextLarge = new(20, 20);
        private FontSpec FontTextSmall = new(14, 14);

        public MinimalProgressBarLayer()
        {
            LoadingText = Love.Graphics.NewText(FontTextLarge.LoveFont, "Now Loading...");
            StatusText = Love.Graphics.NewText(FontTextSmall.LoveFont, string.Empty);
        }

        public override void Initialize(IProgressableJob job)
        {
            Job = job;
            Steps = job.GetEnumerator();

            AdvanceStep();
        }

        private bool AdvanceStep()
        {
            var hasNext = Steps.MoveNext();
            StepNumber++;

            if (hasNext)
            {
                _currentOperation = new ProgressOperation();
                StatusText.Set(Love.ColoredStringArray.Create(Steps.Current.Text));
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
                Finish(new UINone());
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
        }

        public override void Draw()
        {
            Love.Graphics.Clear(Love.Color.Black);

            var x = X + Width / 2;
            var y = Y + Height / 2;
            var barWidth = 400;
            var barHeight = 20;

            Love.Graphics.SetColor(Love.Color.White);
            Love.Graphics.Draw(LoadingText, x - LoadingText.GetWidth() / 2, y - LoadingText.GetHeight() / 2 - 24 - 4 - barHeight);

            Love.Graphics.Draw(StatusText, x - StatusText.GetWidth() / 2, y - StatusText.GetHeight() / 2 - 4 - barHeight);

            Love.Graphics.Rectangle(Love.DrawMode.Line, x - barWidth / 2, y, barWidth, barHeight);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, x - barWidth / 2, y, (int)(barWidth * ProgressPercent), barHeight);

            Love.Graphics.Rectangle(Love.DrawMode.Line, x - barWidth / 2, y + barHeight + 4, barWidth, barHeight);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, x - barWidth / 2, y + barHeight + 4, (int)(barWidth * _currentOperation.Progress), barHeight);
        }

        public override void Dispose()
        {
            LoadingText.Dispose();
            StatusText.Dispose();
        }
    }
}
