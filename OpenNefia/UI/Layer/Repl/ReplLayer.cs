using System;
using CSharpRepl.Services.Completion;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Tags;
using OpenNefia.Core.Game;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Utility;
using TextCopy;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Core.UI.Layer.Repl
{
    public class ReplLayer : BaseUiLayer<UiNoResult>
    {
        protected class ReplTextLine
        {
            public string Text;
            public Color Color;

            public ReplTextLine(string line, Color color)
            {
                Text = line;
                Color = color;
            }
        }

        private float _HeightPercentage = 0.3f;
        public float HeightPercentage
        {
            get
            {
                if (IsFullscreen)
                    return 1.0f;
                return _HeightPercentage;
            }
            set => _HeightPercentage = value;
        }

        private bool _IsFullscreen = false;
        public bool IsFullscreen
        {
            get => _IsFullscreen;
            set
            {
                _IsFullscreen = value;
                GetPreferredBounds(out var bounds);
                SetSize(bounds.Width, bounds.Height);
                SetPosition(bounds.Left, bounds.Top);
            }
        }

        public bool UsePullDownAnimation { get; set; } = true;
        public float PullDownSpeed { get; set; } = 2.5f;
        public bool HideDuringExecute { get; set; } = true;
        public string EditingLine
        {
            get => TextEditingLine.Text;
            set
            {
                TextEditingLine.Text = value;
                CaretPos = Math.Clamp(CaretPos, 0, TextEditingLine.Text.Length);
            }
        }
        public bool ShowCompletions { get; set; } = true;

        public int ScrollbackSize { get => ScrollbackBuffer.Size; }
        public int CursorDisplayX { get => X + 6 + TextCaret.Width + CursorX; }
        public int CursorDisplayY { get => Y + Height - PullDownY - FontReplText.LoveFont.GetHeight() - 4; }

        public FontSpec FontReplText { get; } = UiFonts.ReplText;
        public Color ColorReplBackground { get; } = UiColors.ReplBackground;
        public Color ColorReplText { get; } = UiColors.ReplText;
        public Color ColorReplTextResult { get; } = UiColors.ReplTextResult;
        public Color ColorReplTextError { get; } = UiColors.ReplTextError;

        private IUiText TextCaret;
        private IUiText TextEditingLine;
        private IUiText TextScrollbackCounter;
        private IUiText[] TextScrollback;

        private IReplExecutor Executor;
        private CompletionsPane CompletionsPane;

        protected float Dt = 0f;
        protected bool IsPullingDown = false;
        protected int PullDownY = 0;
        public int MaxLines { get; private set; } = 0;
        protected int CursorX = 0;

        private int _CursorCharPos = 0;
        /// Stringwise width position of cursor. (not CJK width)
        public int CaretPos
        {
            get => _CursorCharPos;
            set
            {
                _CursorCharPos = Math.Clamp(value, 0, EditingLine.Length);
                var prefixToCursor = EditingLine.Substring(0, CaretPos);
                var prefixWidth = FontReplText.LoveFont.GetWidth(prefixToCursor);
                CursorX = prefixWidth;
            }
        }

        protected bool NeedsScrollbackRedraw = true;
        protected CircularBuffer<ReplTextLine> ScrollbackBuffer;
        protected int ScrollbackPos = 0;
        protected List<string> History = new List<string>();
        protected IReadOnlyCollection<CompletionItemWithDescription>? Completions;
        protected int HistoryPos = -1;
        private bool IsExecuting = false;

        public ReplLayer(int scrollbackSize = 15000, IReplExecutor? executor = null)
        {
            TextCaret = new UiText(/*FontReplText, */"> ");
            TextEditingLine = new UiText(/*FontReplText, */"");
            TextScrollbackCounter = new UiText(/*FontReplText, */"0/0");
            TextScrollback = new IUiText[0];
            ScrollbackBuffer = new CircularBuffer<ReplTextLine>(scrollbackSize);

            Executor = executor ?? new CSharpReplExecutor(this);

            CompletionsPane = new CompletionsPane((input, caret) => Executor.Complete(input, caret));

            BindKeys();
        }

        protected virtual void BindKeys()
        {
            TextInput.Enabled = true;
            TextInput.Callback += (evt) => InsertText(evt.Text);

            Keybinds[Keys.Up] += (_) =>
            {
                if (CompletionsPane.IsVisible)
                    CompletionsPane.Decrement();
                else
                    PreviousHistoryEntry();
            };
            Keybinds[Keys.Down] += (_) =>
            {
                if (CompletionsPane.IsVisible)
                    CompletionsPane.Increment();
                else
                    NextHistoryEntry();
            };
            Keybinds[Keys.Left] += (_) =>
            {
                CaretPos -= 1;
                UpdateCompletions();
            };
            Keybinds[Keys.Right] += (_) =>
            {
                CaretPos += 1;
                UpdateCompletions();
            };
            Keybinds[Keys.Backspace] += (_) => DeleteCharAtCursor();
            Keybinds[Keys.PageUp] += (_) => SetScrollbackPos(ScrollbackPos + MaxLines / 2);
            Keybinds[Keys.PageDown] += (_) => SetScrollbackPos(ScrollbackPos - MaxLines / 2);
            Keybinds[Keys.Ctrl | Keys.A] += (_) =>
            {
                CaretPos = 0;
                UpdateCompletions();
            };
            Keybinds[Keys.Ctrl | Keys.E] += (_) =>
            {
                CaretPos = EditingLine.Length;
                UpdateCompletions();
            };
            Keybinds[Keys.Ctrl | Keys.F] += (_) => IsFullscreen = !IsFullscreen;
            Keybinds[Keys.Ctrl | Keys.X] += (_) => CutText();
            Keybinds[Keys.Ctrl | Keys.C] += (_) => CopyText();
            Keybinds[Keys.Ctrl | Keys.V] += (_) => PasteText();
            Keybinds[Keys.Ctrl | Keys.N] += (_) =>
            {
                if (!CompletionsPane.IsOpen)
                    CompletionsPane.Open(CaretPos);
                else
                    CompletionsPane.Increment();
            };
            Keybinds[Keys.Ctrl | Keys.P] += (_) =>
            {
                if (!CompletionsPane.IsOpen)
                    CompletionsPane.Open(CaretPos);
                else
                    CompletionsPane.Decrement();
            };
            Keybinds[Keys.Tab] += (_) => InsertCompletion();
            Keybinds[Keys.Enter] += (_) =>
            {
                if (CompletionsPane.IsVisible)
                    InsertCompletion();
                else
                    SubmitText();
            };
            Keybinds[Keys.Escape] += (_) =>
            {
                if (CompletionsPane.IsVisible)
                    CompletionsPane.Close();
                else
                    Cancel();
            };
        }

        public void InsertText(string inserted)
        {
            if (inserted == string.Empty)
                return;

            EditingLine = EditingLine.Insert(CaretPos, inserted);
            CaretPos += inserted.Length;

            UpdateCompletions();
        }

        public void DeleteCharAtCursor()
        {
            if (CaretPos == 0)
            {
                return;
            }

            var text = EditingLine;
            text = text.Remove(CaretPos - 1, 1);

            CaretPos -= 1;
            EditingLine = text;

            UpdateCompletions();
        }

        public void SetScrollbackPos(int pos)
        {
            ScrollbackPos = Math.Clamp(pos, 0, Math.Max(ScrollbackBuffer.Size - MaxLines, 0));
            NeedsScrollbackRedraw = true;
        }

        public void NextHistoryEntry()
        {
            var search = false;

            if (search)
            {
                // TODO
            }
            else
            {
                if (HistoryPos - 1 < 0)
                {
                    //this.EditingLine = "";
                    //this.HistoryPos = -1;
                }
                else if (HistoryPos - 1 <= History.Count)
                {
                    HistoryPos -= 1;
                    EditingLine = History[HistoryPos];
                    CaretPos = EditingLine.Length;
                }
            }
        }

        public void PreviousHistoryEntry()
        {
            var search = false;

            if (search)
            {
                // TODO
            }
            else
            {
                if (HistoryPos + 1 > History.Count - 1)
                {
                    //this.EditingLine = "";
                    //this.HistoryPos = this.History.Count;
                }
                else if (HistoryPos + 1 <= History.Count)
                {
                    HistoryPos += 1;
                    EditingLine = History[HistoryPos];
                    CaretPos = EditingLine.Length;
                }
            }
        }

        public void CutText()
        {
            ClipboardService.SetText(EditingLine);
            EditingLine = "";
            CaretPos = 0;

            UpdateCompletions();
        }

        public void CopyText()
        {
            ClipboardService.SetText(EditingLine);
        }

        public void PasteText()
        {
            var text = ClipboardService.GetText() ?? "";
            InsertText(text);
        }

        private void InsertCompletion()
        {
            if (!CompletionsPane.IsOpen)
                return;

            var completion = CompletionsPane.SelectedItem;
            if (completion == null)
                return;

            var text = EditingLine;

            var completeAgain = false;
            var insertText = completion.Item.DisplayText;

            var tags = completion.Item.Tags;
            if (tags.Contains(WellKnownTags.Namespace))
            {
                insertText += ".";
                completeAgain = true;
            }
            else if (completion.Item.Properties.GetValueOrDefault("ShouldProvideParenthesisCompletion") == "True")
            {
                // insertText += "(";
            }

            text = text.Remove(completion.Item.Span.Start, CaretPos - completion.Item.Span.Start);
            text = text.Insert(completion.Item.Span.Start, insertText);
            EditingLine = text;
            CaretPos = completion.Item.Span.Start + insertText.Length;
            CompletionsPane.Close();

            if (completeAgain)
                UpdateCompletions();
        }

        private void UpdateCompletions()
        {
            Dt = 0f;

            if (!ShowCompletions && CompletionsPane.IsOpen)
            {
                CompletionsPane.Close();
                return;
            }

            CompletionsPane.SetPosition(CursorDisplayX, CursorDisplayY + FontReplText.LoveFont.GetHeight());
            CompletionsPane.TryToComplete(EditingLine, CaretPos);
        }

        public void Clear()
        {
            ScrollbackBuffer.Clear();
            ScrollbackPos = 0;
            UpdateCompletions();
            NeedsScrollbackRedraw = true;
        }

        public void SubmitText()
        {
            var code = TextEditingLine.Text;

            CompletionsPane.Close();

            if (code != string.Empty)
            {
                History.Insert(0, code);
            }

            TextEditingLine.Text = string.Empty;
            ScrollbackPos = 0;
            HistoryPos = -1;
            CaretPos = 0;
            CursorX = 0;
            Dt = 0;

            PrintText($"{TextCaret.Text}{code}");

            IsExecuting = true;
            var result = Executor.Execute(code);
            IsExecuting = false;

            switch (result)
            {
                case ReplExecutionResult.Success success:
                    PrintText(success.Result, ColorReplTextResult);
                    break;
                case ReplExecutionResult.Error error:
                    var text = $"Error: {error.Exception.Message}";
                    PrintError(text);
                    break;
                default:
                    break;
            }

            GameSession.Field.RefreshScreen();
        }

        public void PrintError(string text) => PrintText(text, ColorReplTextError);

        public void PrintText(string text, Color? color = null)
        {
            if (color == null)
                color = ColorReplText;

            var (_, wrapped) = FontReplText.LoveFont.GetWrap(text, Width);

            foreach (var line in wrapped)
            {
                ScrollbackBuffer.PushFront(new ReplTextLine(line, color.Value));
            }

            NeedsScrollbackRedraw = true;
        }

        public override void GetPreferredBounds(out UIBox2i bounds)
        {
            var viewportHeight = Love.Graphics.GetHeight();

            bounds = UIBox2i.FromDimensions(0, 0, Love.Graphics.GetWidth(), (int)Math.Clamp(viewportHeight * HeightPercentage, 0, viewportHeight - 1));
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            MaxLines = (Height - 5) / FontReplText.LoveFont.GetHeight();

            foreach (var text in TextScrollback)
                text.Dispose();
            TextScrollback = new IUiText[MaxLines];
            for (int i = 0; i < MaxLines; i++)
                TextScrollback[i] = new UiText(FontReplText);

            NeedsScrollbackRedraw = true;
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
        }

        public override void OnQuery()
        {
            Executor.Initialize();

            IsPullingDown = UsePullDownAnimation;
            PullDownY = 0;

            if (UsePullDownAnimation)
            {
                PullDownY = MaxLines * FontReplText.LoveFont.GetHeight();
            }
        }

        public override void Update(float dt)
        {
            Dt += dt;

            TextCaret.Update(dt);
            TextEditingLine.Update(dt);
            TextScrollbackCounter.Update(dt);
            foreach (var text in TextScrollback)
            {
                text.Update(dt);
            }

            CompletionsPane.Update(dt);

            if (UsePullDownAnimation)
            {
                if (WasFinished || WasCancelled)
                {
                    PullDownY = (int)Math.Min(PullDownY + PullDownSpeed * dt * 1000f, MaxLines * FontReplText.LoveFont.GetHeight());
                }
                else if (PullDownY > 0)
                {
                    PullDownY = (int)Math.Max(PullDownY - PullDownSpeed * dt * 1000f, 0);
                }
            }
        }

        public override UiResult<UiNoResult>? GetResult()
        {
            if (!UsePullDownAnimation)
                return base.GetResult();

            if (WasFinished || WasCancelled)
            {
                if (PullDownY >= MaxLines * FontReplText.LoveFont.GetHeight())
                {
                    return base.GetResult();
                }
            }

            return null;
        }

        public override void Draw()
        {
            if (IsExecuting && HideDuringExecute)
            {
                return;
            }

            var y = Y - PullDownY;

            // Background
            GraphicsEx.SetColor(ColorReplBackground);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, X, y, Width, Height);

            var yPos = y + Height - FontReplText.LoveFont.GetHeight() - 5;

            // Caret
            if (!IsExecuting)
            {
                TextCaret.SetPosition(X + 5, yPos);
                TextCaret.Draw();
            }

            // Current line
            TextEditingLine.SetPosition(X + 5 + FontReplText.LoveFont.GetWidth(TextCaret.Text), yPos);
            TextEditingLine.Draw();

            // Scrollback Display
            if (NeedsScrollbackRedraw)
            {
                if (ScrollbackPos > 0)
                {
                    TextScrollbackCounter.Text = $"{ScrollbackPos}/{ScrollbackBuffer.Size}";
                    TextScrollbackCounter.SetPosition(X + Width - TextScrollbackCounter.Width - 5, yPos);
                }

                for (int i = 0; i < MaxLines; i++)
                {
                    var index = ScrollbackPos + i;
                    if (index >= ScrollbackBuffer.Size)
                    {
                        break;
                    }

                    var uiText = TextScrollback[i];
                    var line = ScrollbackBuffer[index];
                    uiText.Text = line.Text;
                    uiText.Color = line.Color;
                }
                NeedsScrollbackRedraw = false;
            }

            // Scrollback counter
            if (ScrollbackPos > 0)
            {
                TextScrollbackCounter.Draw();
            }

            for (int i = 0; i < TextScrollback.Length; i++)
            {
                var text = TextScrollback[i];
                text.SetPosition(X + 5, y + Height - FontReplText.LoveFont.GetHeight() * (i + 2) - 5);
                text.Draw();
            }

            if (Math.Floor(Dt * 2) % 2 == 0 && IsQuerying())
            {
                var curX = CursorDisplayX;
                var curY = CursorDisplayY;
                GraphicsEx.SetColor(ColorReplText);
                Love.Graphics.Line(curX, curY, curX, curY + FontReplText.LoveFont.GetHeight());
            }

            CompletionsPane.Draw();
        }

        public override void Dispose()
        {
            TextCaret.Dispose();
            TextEditingLine.Dispose();
            TextScrollbackCounter.Dispose();
            foreach (var text in TextScrollback)
            {
                text.Dispose();
            }
            CompletionsPane.Dispose();
        }
    }

    public delegate IReadOnlyCollection<CompletionItemWithDescription> CompletionCallback(string input, int caret);

    public class CompletionsPane : BaseUiElement
    {
        public int Padding { get; set; } = 5;
        public int BorderPadding { get; set; } = 4;
        public int MaxDisplayedEntries { get; set; } = 10;
        public bool IsOpen { get; set; }
        public bool IsVisible { get => IsOpen && FilteredView.Count > 0; }

        public CompletionItemWithDescription? SelectedItem { get => FilteredView.SelectedItem?.Completion; }

        private record CompletionPaneEntry(IUiText Text,
                                           IAssetDrawable Icon,
                                           CompletionItemWithDescription Completion);

        private List<CompletionPaneEntry> Entries;
        private SlidingArrayWindow<CompletionPaneEntry> FilteredView;
        private int CaretPosWhenOpened = int.MinValue;
        private CompletionCallback Callback;

        public FontSpec FontCompletion { get; } = UiFonts.ReplCompletion;
        public Color ColorCompletionBorder { get; } = UiColors.ReplCompletionBorder;
        public Color ColorCompletionBackground { get; } = UiColors.ReplCompletionBackground;
        internal ReplCompletionIcons AssetIcons { get; }

        public CompletionsPane(CompletionCallback callback)
        {
            Entries = new List<CompletionPaneEntry>();
            FilteredView = new SlidingArrayWindow<CompletionPaneEntry>();
            Callback = callback;

            // FontCompletion = FontDefOf.ReplCompletion;
            // ColorCompletionBorder = ColorDefOf.ReplCompletionBorder;
            // ColorCompletionBackground = ColorDefOf.ReplCompletionBackground;
            AssetIcons = new ReplCompletionIcons();
        }

        public void Open(int caret)
        {
            IsOpen = true;
            CaretPosWhenOpened = caret;
            Clear();
        }

        private void Clear()
        {
            foreach (var item in Entries)
                item.Text.Dispose();
            Entries.Clear();
            FilteredView = new SlidingArrayWindow<CompletionPaneEntry>();
        }

        public void Close()
        {
            IsOpen = false;
            CaretPosWhenOpened = int.MinValue;
            FilteredView = new SlidingArrayWindow<CompletionPaneEntry>();
        }

        public void SetFromCompletions(IReadOnlyCollection<CompletionItemWithDescription> completions, string input, int caret)
        {
            Clear();

            foreach (var completion in completions)
            {
                Entries.Add(new CompletionPaneEntry(new UiText(FontCompletion, completion.Item.DisplayText),
                                                    AssetIcons.GetIcon(completion.Item.Tags),
                                                    completion));
            }

            FilterCompletions(input, caret);
        }

        public override void GetPreferredSize(out Vector2i size)
        {
            size = Vector2i.Zero;

            foreach (var entry in FilteredView)
            {
                entry.Text.SetPreferredSize();
                size.X = Math.Max(entry.Text.Width + Padding * 2 + entry.Text.Height + 4, size.X);
                size.Y += entry.Text.Height;
            }

            size.X += Padding * 2;
            size.Y += Padding * 2 + BorderPadding * 2;
        }

        public void Increment()
        {
            FilteredView.IncrementSelectedIndex();
            SetPreferredSize();
            SetPosition(X, Y);
        }

        public void Decrement()
        {
            FilteredView.DecrementSelectedIndex();
            SetPreferredSize();
            SetPosition(X, Y);
        }

        public void FilterCompletions(string input, int caret)
        {
            bool Matches(CompletionItemWithDescription completion, string input) =>
                completion.Item.DisplayText.StartsWith(input.Substring(completion.Item.Span.Start), StringComparison.CurrentCultureIgnoreCase);

            var filtered = new List<CompletionPaneEntry>();
            var previouslySelectedItem = FilteredView.SelectedItem;
            var selectedIndex = -1;
            for (var i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];
                if (!Matches(entry.Completion, input)) continue;

                filtered.Add(entry);
                if (entry.Completion.Item.DisplayText == previouslySelectedItem?.Completion.Item.DisplayText)
                {
                    selectedIndex = filtered.Count - 1;
                }
            }
            if (selectedIndex == -1 || previouslySelectedItem == null || !Matches(previouslySelectedItem!.Completion, input))
            {
                selectedIndex = 0;
            }
            FilteredView = new SlidingArrayWindow<CompletionPaneEntry>(
                filtered.ToArray(),
                MaxDisplayedEntries,
                selectedIndex
            );

            SetPreferredSize();
            SetPosition(X, Y);
        }

        public void TryToComplete(string input, int caret)
        {
            if (ShouldAutomaticallyOpen(input, caret) is int offset and >= 0)
            {
                Close();
                Open(caret - offset);
            }

            if (caret < CaretPosWhenOpened || string.IsNullOrWhiteSpace(input))
            {
                Clear();
            }
            else if (IsOpen)
            {
                if (Entries.Count == 0)
                {
                    var completions = Callback(input, caret);
                    if (completions.Any())
                    {
                        SetFromCompletions(completions, input, caret);
                    }
                    else
                    {
                        Close();
                    }
                }
                else
                {
                    FilterCompletions(input, caret);
                    if (HasTypedPastCompletion(caret))
                    {
                        Close();
                    }
                }
            }
        }

        private static int ShouldAutomaticallyOpen(string input, int caret)
        {
            if (caret > 0 && input[caret - 1] is '.' or '(') return 0; // typical "intellisense behavior", opens for new methods and parameters

            if (caret == 1 && !char.IsWhiteSpace(input[0]) // 1 word character typed in brand new prompt
                && (input.Length == 1 || !char.IsLetterOrDigit(input[1]))) // if there's more than one character on the prompt, but we're typing a new word at the beginning (e.g. "a| bar")
            {
                return 1;
            }

            // open when we're starting a new "word" in the prompt.
            return caret - 2 >= 0
                && char.IsWhiteSpace(input[caret - 2])
                && char.IsLetter(input[caret - 1])
                ? 1
                : -1;
        }

        private bool HasTypedPastCompletion(int caret) =>
            FilteredView.SelectedItem is not null
            && FilteredView.SelectedItem.Completion.Item.DisplayText.Length < caret - CaretPosWhenOpened;


        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            foreach (var (entry, index) in FilteredView.WithIndex())
            {
                entry.Text.SetPosition(x + Padding + BorderPadding + entry.Text.Height + 4, y + Padding + BorderPadding + index * FontCompletion.LoveFont.GetHeight());
            }
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            if (!IsVisible)
                return;

            GraphicsEx.SetColor(ColorCompletionBackground);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, X, Y, Width, Height);

            GraphicsEx.SetColor(ColorCompletionBorder);
            Love.Graphics.Rectangle(Love.DrawMode.Line, X + BorderPadding, Y + BorderPadding, Width - BorderPadding * 2, Height - BorderPadding * 2);

            foreach (var entry in FilteredView)
            {
                if (entry == FilteredView.SelectedItem)
                {
                    GraphicsEx.SetColor(255, 255, 255, 128);
                    Love.Graphics.Rectangle(Love.DrawMode.Fill, entry.Text.X, entry.Text.Y, entry.Text.Width, entry.Text.Height);
                }
                GraphicsEx.SetColor(Color.White);
                entry.Icon.Draw(entry.Text.X - entry.Text.Height - 4, entry.Text.Y, entry.Text.Height, entry.Text.Height);
                entry.Text.Draw();
            }
        }
    }
}
