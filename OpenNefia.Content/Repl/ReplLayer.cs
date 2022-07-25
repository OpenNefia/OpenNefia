using CSharpRepl.Services.Completion;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Tags;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Console;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.Utility;
using TextCopy;
using Color = OpenNefia.Core.Maths.Color;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Input;
using OpenNefia.Content.Input;
using OpenNefia.Core.UserInterface;
using YamlDotNet.Core.Tokens;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Profiles;
using OpenNefia.Core.Log;
using OpenNefia.Content.UI.Hud;

namespace OpenNefia.Content.Repl
{
    public interface IReplLayer : IUiLayerWithResult<UINone, UINone>
    {
        int ScrollbackSize { get; }
        FontSpec FontReplText { get; }
        int MaxLines { get; }

        void Initialize();
        void Clear();
        void PrintText(string text, Color? color = null);
    }

    public class ReplLayer : UiLayerWithResult<UINone, UINone>, IReplLayer
    {
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IReplExecutor _executor = default!;
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IInputManager _inputManager = default!;
        [Dependency] private readonly IProfileManager _profiles = default!;

        public const string HistoryFile = "/ReplHistory.txt";
        private const int MaxSavedHistoryLines = 500;

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

        public int ScrollbackSize { get => _scrollbackBuffer.Size; }
        public float CursorDisplayX { get => X + TextCaret.Width + TextEditingLine.TextWidth + TextEditingLine.Font.LoveFont.GetWidthV(UIScale, " "); }
        public float CursorDisplayY { get => Y + Height - PullDownY - FontReplText.LoveFont.GetHeightV(UIScale) - 4; }

        public FontSpec FontReplText { get; } = UiFonts.ReplText;
        public Color ColorReplBackground { get; } = UiColors.ReplBackground;
        public Color ColorReplText { get; } = UiColors.ReplText;
        public Color ColorReplTextResult { get; } = UiColors.ReplTextResult;
        public Color ColorReplTextError { get; } = UiColors.ReplTextError;

        [Child] private UiText TextCaret;
        [Child] private UiText TextEditingLine;
        [Child] private UiText TextScrollbackCounter;
        private UiText[] TextScrollback;

        private CompletionsPane _completionsPane;

        protected float Dt = 0f;

        protected bool IsPullingDown = false;
        protected float PullDownY = 0f;
        protected float PullDownPixelY => PullDownY * UIScale;

        protected float CursorX = 0f;
        protected float CursorPixelX => CursorX * UIScale;

        public int MaxLines { get; private set; } = 0;

        private int _CursorCharPos = 0;
        /// Stringwise width position of cursor. (not CJK width)
        public int CaretPos
        {
            get => _CursorCharPos;
            set
            {
                _CursorCharPos = Math.Clamp(value, 0, EditingLine.Length);
                var prefixToCursor = EditingLine.Substring(0, CaretPos);
                var prefixWidth = FontReplText.LoveFont.GetWidthV(UIScale, prefixToCursor);
                CursorX = prefixWidth;
            }
        }

        protected bool NeedsScrollbackRedraw = true;
        protected CircularBuffer<ReplTextLine> _scrollbackBuffer;
        protected int ScrollbackPos = 0;
        protected List<string> History = new List<string>();
        protected IReadOnlyCollection<CompletionItemWithDescription>? Completions;
        protected int HistoryPos = -1;
        private bool IsExecuting = false;
        private bool _wasInitialized;

        public override int? DefaultZOrder => HudLayer.HudZOrder + 1000000;

        public ReplLayer()
        {
            TextCaret = new UiText(FontReplText, "> ");
            TextEditingLine = new UiText(FontReplText, "");
            TextScrollbackCounter = new UiText(FontReplText, "0/0");
            TextScrollback = new UiText[0];

            _scrollbackBuffer = new CircularBuffer<ReplTextLine>(10000);
            _completionsPane = new CompletionsPane((input, caret) => _executor.Complete(input, caret));

            CanControlFocus = true;
            CanKeyboardFocus = true;
            OnKeyBindDown += HandleKeyBindDown;
        }

        public void Initialize()
        {
            _inputManager.KeyBindStateChanged += HandleGlobalKeyBindStateChanged;
            LoadHistory();
        }

        private void LoadHistory()
        {
            var historyPath = new ResourcePath(HistoryFile);
            if (_profiles.CurrentProfile.Exists(historyPath))
            {
                Logger.InfoS("repl", $"Loading REPL history from {historyPath}");

                using var text = _profiles.CurrentProfile.OpenText(historyPath);
                History = text.ReadLines().ToList();
            }
        }

        private void SaveHistory()
        {
            var historyPath = new ResourcePath(HistoryFile);
            var allLines = string.Join('\n', History.Take(MaxSavedHistoryLines));
            _profiles.CurrentProfile.WriteAllText(historyPath, allLines);
        }

        /// <summary>
        /// The REPL layer can be called under any context, whether it be in the title
        /// screen or in-game. This keybind handler hooks directly into the input manager
        /// to accomplish this.
        /// </summary>
        private void HandleGlobalKeyBindStateChanged(ViewportBoundKeyEventArgs args)
        {
            if (args.KeyEventArgs.Function != EngineKeyFunctions.ShowDebugConsole)
                return;

            args.KeyEventArgs.Handle();

            if (IsInActiveLayerList())
            {
                // REPL is already open.
                return;
            }

            var uiManager = IoCManager.Resolve<IUserInterfaceManager>();
            uiManager.InitializeLayer<ReplLayer, UINone, UINone>(this, new());
            uiManager.Query(this);
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.TextHistoryPrev)
            {
                if (_completionsPane.IsVisible)
                    _completionsPane.Decrement();
                else
                    PreviousHistoryEntry();

            }
            else if (args.Function == EngineKeyFunctions.TextHistoryNext)
            {
                if (_completionsPane.IsVisible)
                    _completionsPane.Increment();
                else
                    NextHistoryEntry();
            }
            else if (args.Function == EngineKeyFunctions.TextCursorLeft)
            {
                CaretPos -= 1;
                UpdateCompletions();
            }
            else if (args.Function == EngineKeyFunctions.TextCursorRight)
            {
                CaretPos += 1;
                UpdateCompletions();
            }
            else if (args.Function == EngineKeyFunctions.TextBackspace)
            {
                DeleteCharAtCursor();
            }
            else if (args.Function == EngineKeyFunctions.TextPageUp)
            {
                SetScrollbackPos(ScrollbackPos + MaxLines / 2);
            }
            else if (args.Function == EngineKeyFunctions.TextPageDown)
            {
                SetScrollbackPos(ScrollbackPos - MaxLines / 2);
            }
            else if (args.Function == EngineKeyFunctions.TextCursorBegin)
            {
                CaretPos = 0;
                UpdateCompletions();
            }
            else if (args.Function == EngineKeyFunctions.TextCursorEnd)
            {
                CaretPos = EditingLine.Length;
                UpdateCompletions();
            }
            else if (args.Function == ContentKeyFunctions.ReplFullscreen)
            {
                IsFullscreen = !IsFullscreen;
            }
            else if (args.Function == EngineKeyFunctions.TextCut)
            {
                CutText();
            }
            else if (args.Function == EngineKeyFunctions.TextCopy)
            {
                CopyText();
            }
            else if (args.Function == EngineKeyFunctions.TextPaste)
            {
                PasteText();
            }
            else if (args.Function == ContentKeyFunctions.ReplPrevCompletion)
            {
                if (!_completionsPane.IsOpen)
                    _completionsPane.Open(CaretPos);
                else
                    _completionsPane.Decrement();
            }
            else if (args.Function == ContentKeyFunctions.ReplNextCompletion)
            {
                if (!_completionsPane.IsOpen)
                    _completionsPane.Open(CaretPos);
                else
                    _completionsPane.Increment();
            }
            else if (args.Function == ContentKeyFunctions.ReplComplete)
            {
                InsertCompletion(submitPressed: false);
            }
            else if (args.Function == ContentKeyFunctions.ReplClear)
            {
                Clear();
            }
            else if (args.Function == EngineKeyFunctions.TextSubmit)
            {
                if (_completionsPane.IsVisible)
                    InsertCompletion(submitPressed: true);
                else
                    SubmitText();
            }
            else if (args.Function == EngineKeyFunctions.TextReleaseFocus)
            {
                if (_completionsPane.IsVisible)
                    _completionsPane.Close();
                else
                    Cancel();
            }
        }

        protected override void TextEntered(GUITextEventArgs args)
        {
            InsertText(args.AsRune.ToString());
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
            ScrollbackPos = Math.Clamp(pos, 0, Math.Max(_scrollbackBuffer.Size - MaxLines, 0));
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
                    EditingLine = "";
                    HistoryPos = -1;
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
                    EditingLine = "";
                    HistoryPos = History.Count;
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

        private void InsertCompletion(bool submitPressed)
        {
            if (!_completionsPane.IsOpen)
                return;

            var completion = _completionsPane.SelectedItem;
            if (completion == null)
                return;

            var originalText = EditingLine;
            var text = originalText;

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
                insertText += "(";
            }

            text = text.Remove(completion.Item.Span.Start, CaretPos - completion.Item.Span.Start);
            text = text.Insert(completion.Item.Span.Start, insertText);
            EditingLine = text;
            CaretPos = completion.Item.Span.Start + insertText.Length;
            _completionsPane.Close();

            if (completeAgain)
                UpdateCompletions();

            // If the completion matches the current input exactly, submit the completed text if
            // Enter was pressed (and not if Tab was pressed)

            if (submitPressed)
            {
                var exactMatch = string.Equals(originalText.Substring(completion.Item.Span.Start), completion.Item.DisplayText, StringComparison.InvariantCultureIgnoreCase);

                var shouldSubmitCompletion = tags.Contains(WellKnownTags.Field)
                    || tags.Contains(WellKnownTags.Property)
                    || tags.Contains(WellKnownTags.Constant);

                if (exactMatch && shouldSubmitCompletion)
                    SubmitText();
            }
        }

        private void UpdateCompletions()
        {
            Dt = 0f;

            if (!ShowCompletions && _completionsPane.IsOpen)
            {
                _completionsPane.Close();
                return;
            }

            _completionsPane.SetPosition(CursorDisplayX, CursorDisplayY + FontReplText.LoveFont.GetHeightV(UIScale));
            _completionsPane.TryToComplete(EditingLine, CaretPos);
        }

        public void Clear()
        {
            _scrollbackBuffer.Clear();
            ScrollbackPos = 0;
            UpdateCompletions();
            NeedsScrollbackRedraw = true;
        }

        public void SubmitText()
        {
            var code = TextEditingLine.Text;

            _completionsPane.Close();

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
            var result = _executor.Execute(code);
            IsExecuting = false;

            switch (result)
            {
                case ReplExecutionResult.Success success:
                    PrintText(success.Result, ColorReplTextResult);
                    break;
                case ReplExecutionResult.Error error:
                    var text = $"Error: {error.Exception.Message}";
                    PrintText(text, ColorReplTextError);
                    break;
                default:
                    break;
            }

            SaveHistory();

            _field.RefreshScreen();
        }

        public void PrintText(string text, Color? color = null)
        {
            if (color == null)
                color = ColorReplText;

            var (_, wrapped) = FontReplText.LoveFont.GetWrap(text, Width);

            foreach (var line in wrapped)
            {
                _scrollbackBuffer.PushFront(new ReplTextLine(line, color.Value));
            }

            NeedsScrollbackRedraw = true;
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            var viewportHeight = _graphics.WindowSize.Y;

            bounds = UIBox2.FromDimensions(0, 0, _graphics.WindowSize.X, (int)Math.Clamp(viewportHeight * HeightPercentage, 0, viewportHeight - 1));
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            MaxLines = (int)((Height - 5) / FontReplText.LoveFont.GetHeightV(UIScale));

            foreach (var text in TextScrollback)
            {
                text.Dispose();
                RemoveChild(text);
            }
            TextScrollback = new UiText[MaxLines];
            for (int i = 0; i < MaxLines; i++)
            {
                TextScrollback[i] = new UiText(FontReplText);
                UiHelpers.AddChildrenRecursive(this, TextScrollback[i]);
            }

            NeedsScrollbackRedraw = true;
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
        }

        private void InitializeExecutor()
        {
            _executor.Initialize();
            _wasInitialized = true;
        }

        public override void OnQuery()
        {
            if (!_wasInitialized)
            {
                InitializeExecutor();
            }

            IsPullingDown = UsePullDownAnimation;
            PullDownY = 0f;

            if (UsePullDownAnimation)
            {
                PullDownY = MaxLines * FontReplText.LoveFont.GetHeightV(UIScale);
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

            _completionsPane.Update(dt);

            if (UsePullDownAnimation)
            {
                if (WasFinished || WasCancelled)
                {
                    PullDownY = MathF.Min(PullDownY + PullDownSpeed * dt * 1000f, MaxLines * FontReplText.LoveFont.GetHeightV(UIScale));
                }
                else if (PullDownY > 0)
                {
                    PullDownY = MathF.Max(PullDownY - PullDownSpeed * dt * 1000f, 0);
                }
            }
        }

        public override UiResult<UINone>? GetResult()
        {
            if (!UsePullDownAnimation)
                return base.GetResult();

            if (WasFinished || WasCancelled)
            {
                if (PullDownY >= MaxLines * FontReplText.LoveFont.GetHeightV(UIScale))
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
            GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, X, y, Width, Height);

            var yPos = y + Height - FontReplText.LoveFont.GetHeightV(UIScale) - 5;

            // Caret
            if (!IsExecuting)
            {
                TextCaret.SetPosition(X + 5, yPos);
                TextCaret.Draw();
            }

            // Current line
            TextEditingLine.SetPosition(X + 5 + FontReplText.LoveFont.GetWidthV(UIScale, TextCaret.Text), yPos);
            TextEditingLine.Draw();

            // Scrollback Display
            if (NeedsScrollbackRedraw)
            {
                if (ScrollbackPos > 0)
                {
                    TextScrollbackCounter.Text = $"{ScrollbackPos}/{_scrollbackBuffer.Size}";
                    TextScrollbackCounter.SetPosition(X + Width - TextScrollbackCounter.Width - 5, yPos);
                }

                for (int i = 0; i < MaxLines; i++)
                {
                    var index = ScrollbackPos + i;
                    if (index >= _scrollbackBuffer.Size)
                    {
                        break;
                    }

                    var uiText = TextScrollback[i];
                    var line = _scrollbackBuffer[index];
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
                text.SetPosition(X + 5, y + Height - FontReplText.LoveFont.GetHeightV(UIScale) * (i + 2) - 5);
                text.Draw();
            }

            if (Math.Floor(Dt * 2) % 2 == 0 && IsQuerying())
            {
                var curX = CursorDisplayX;
                var curY = CursorDisplayY;
                GraphicsEx.SetColor(ColorReplText);
                GraphicsS.LineS(UIScale, curX, curY, curX, curY + FontReplText.LoveFont.GetHeightV(UIScale));
            }

            _completionsPane.Draw();
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
            _completionsPane.Dispose();
        }
    }

    public delegate IReadOnlyCollection<CompletionItemWithDescription> CompletionCallback(string input, int caret);

    public class CompletionsPane : UiElement
    {
        public float Padding { get; set; } = 5f;
        public float BorderPadding { get; set; } = 4f;
        public int MaxDisplayedEntries { get; set; } = 10;
        public bool IsOpen { get; set; }
        public bool IsVisible { get => IsOpen && FilteredView.Count > 0; }

        public CompletionItemWithDescription? SelectedItem { get => FilteredView.SelectedItem?.Completion; }

        private record CompletionPaneEntry(UiText Text,
                                           IAssetInstance Icon,
                                           CompletionItemWithDescription Completion);

        private List<CompletionPaneEntry> Entries;
        private SlidingArrayWindow<CompletionPaneEntry> FilteredView;
        private int CaretPosWhenOpened = int.MinValue;
        private CompletionCallback Callback;
        private string _lastInput = string.Empty;
        private bool _isExactMatch = false;

        public FontSpec FontCompletion { get; } = UiFonts.ReplCompletion;
        public Color ColorCompletionBorder { get; } = UiColors.ReplCompletionBorder;
        public Color ColorCompletionBackground { get; } = UiColors.ReplCompletionBackground;
        public Color ColorCompletionExactMatchBackground { get; } = UiColors.ReplCompletionExactMatchBackground;
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
            _lastInput = string.Empty;
            _isExactMatch = false;
        }

        public void Close()
        {
            IsOpen = false;
            CaretPosWhenOpened = int.MinValue;
            FilteredView = new SlidingArrayWindow<CompletionPaneEntry>();
            _lastInput = string.Empty;
            _isExactMatch = false;
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

        public override void GetPreferredSize(out Vector2 size)
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

        private void UpdateExactMatch()
        {
            var selected = FilteredView.SelectedItem;

            if (selected == null)
            {
                _isExactMatch = false;
                return;
            }

            var item = selected.Completion.Item;
            _isExactMatch = string.Equals(_lastInput.Substring(item.Span.Start), item.DisplayText, StringComparison.InvariantCultureIgnoreCase);
        }

        public void Increment()
        {
            FilteredView.IncrementSelectedIndex();
            UpdateExactMatch();
            SetPreferredSize();
            SetPosition(X, Y);
        }

        public void Decrement()
        {
            FilteredView.DecrementSelectedIndex();
            UpdateExactMatch();
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

            _lastInput = input;
            UpdateExactMatch();

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
            if (caret > 0 && input[caret - 1] is '.' or '(' or '<') return 0; // typical "intellisense behavior", opens for new methods and parameters

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


        public override void SetPosition(float x, float y)
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
            GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, X, Y, Width, Height);

            GraphicsEx.SetColor(ColorCompletionBorder);
            GraphicsS.RectangleS(UIScale, Love.DrawMode.Line, X + BorderPadding, Y + BorderPadding, Width - BorderPadding * 2, Height - BorderPadding * 2);

            foreach (var entry in FilteredView)
            {
                if (entry == FilteredView.SelectedItem)
                {
                    if (_isExactMatch)
                        GraphicsEx.SetColor(Color.RoyalBlue.WithAlphaB(128));
                    else
                        GraphicsEx.SetColor(Color.White.WithAlphaB(128));
                    GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, entry.Text.X, entry.Text.Y, entry.Text.Width, entry.Text.Height);
                }

                GraphicsEx.SetColor(Color.White);
                entry.Icon.Draw(UIScale, entry.Text.X - entry.Text.Height - 4, entry.Text.Y, entry.Text.Height, entry.Text.Height);

                if (entry == FilteredView.SelectedItem && _isExactMatch)
                    entry.Text.Color = Color.NavajoWhite;
                else
                    entry.Text.Color = Color.White;
                entry.Text.Draw();
            }
        }
    }
}
