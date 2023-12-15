using OpenNefia.Content.Dialog;
using OpenNefia.Core.Maps;
using OpenNefia.Core;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.UI;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Logic;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Input;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Maths;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.ContentPack;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI;
using OpenNefia.Content.Portraits;

namespace OpenNefia.Content.Scene
{
    public sealed class SceneLayer : UiLayerWithResult<SceneLayer.Args, UINone>, ISceneEngine
    {
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IMusicManager _music = default!;

        public class Args
        {
            public List<ISceneNode> Nodes { get; }

            public Args(IEnumerable<ISceneNode> nodes)
            {
                Nodes = nodes.ToList(); ;
            }
        }

        private enum SceneState
        {
            Advancing,
            AwaitingInput,
            InputSent,
            CrossFadingToDialog,
            Dialog,
            ShowingDialog,
            FadingOut,
            FadingIn,
            Waiting,
            CrossFading,
            Ending,
            Done
        }

        private class DialogData
        {
            public string ActorID { get; }
            public IList<string> Texts { get; }
            public int TextIndex { get; set; }

            public DialogData(string actorID, IList<string> texts)
            {
                this.ActorID = actorID;
                this.Texts = texts;
                this.TextIndex = 0;
            }
        }

        private class TextData
        {
            public IList<string> Texts { get; }
            public int TextIndex { get; set; }

            public TextData(IList<string> texts)
            {
                Texts = texts;
                TextIndex = 0;
            }
        }

        private SceneState _state = SceneState.Advancing;
        private Dictionary<string, SceneActorSpec> _actors = new();
        private IAssetInstance _background = default!;

        [Child] private MorePrompt _morePrompt = new();

        private IList<SceneDialogText>? _dialog = null;
        private UiText[] _textLines = { };
        private TextData? _texts = null;
        private bool _shownAnything = false;

        private float _waitTimer = 0f;
        private float _crossFadeStart = 0f;
        private bool _canvasRebake = false;
        private Love.Image? _crossFadeImage;
        private IAssetInstance _assetSceneTextShadow = default!;

        private IList<ISceneNode> _nodes = new List<ISceneNode>();
        private PrototypeId<MusicPrototype>? _previousMusicID;
        private int currentNodeIndex = 0;
        private ISceneNode? CurrentNode => currentNodeIndex >= _nodes.Count ? null : _nodes[currentNodeIndex];

        public override int? DefaultZOrder => HudLayer.HudZOrder + 100000;

        public SceneLayer()
        {
            EventFilter = UIEventFilterMode.Stop;
            CanControlFocus = true;
            OnKeyBindDown += HandleKeyBindDown;
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (_state != SceneState.AwaitingInput)
            {
                args.Handle();
                return;
            }

            if (args.Function == EngineKeyFunctions.UISelect)
            {
                _morePrompt.IsPromptHidden = true;
                _audio.Play(Protos.Sound.Ok1);
                _state = SceneState.InputSent;
                args.Handle();
            }
            else if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
                args.Handle();
            }
        }

        private void SetCrossFadeParameters(float time = 0.3f)
        {
            _waitTimer = time;
            _crossFadeStart = _waitTimer;
        }

        private void UpdateCrossFadeImage()
        {
            var data = _graphics.CaptureCanvasImageData();
            if (data != null)
                _crossFadeImage = Love.Graphics.NewImage(data);
        }

        public override void Initialize(Args args)
        {
            _nodes = args.Nodes;
            _background = Assets.Get(Protos.Asset.Bg1);
            _assetSceneTextShadow = Assets.Get(Protos.Asset.SceneTextShadow);
            UserInterfaceManager.InitializeLayer(_morePrompt);
            _morePrompt.IsPromptHidden = true;
        }

        public override void OnQuery()
        {
            _previousMusicID = _music.PlayingMusicID;
            currentNodeIndex = 0;
            _crossFadeStart = 0;
            _waitTimer = 0;
            UpdateCrossFadeImage();
            _state = SceneState.Advancing;
        }

        public override void OnQueryFinish()
        {
            if (_previousMusicID != null)
                _music.Play(_previousMusicID.Value);
            else
                _music.Stop();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            UpdateTextPosition();
            _morePrompt.SetPreferredPosition();
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            UpdateTextPosition();
            _morePrompt.SetPreferredSize();
        }

        public void SetActors(IDictionary<string, SceneActorSpec> actors)
        {
            _actors = new Dictionary<string, SceneActorSpec>(actors);
        }

        public void SetBackground(PrototypeId<AssetPrototype> assetID)
        {
            _background = Assets.Get(assetID);
            SetCrossFadeParameters();
        }

        public void ShowText(IList<string> texts)
        {
            if (texts.Count == 0)
            {
                Logger.ErrorS("scene", $"No texts to display!");
                _state = SceneState.Advancing;
                return;
            }

            _shownAnything = true;
            SetCrossFadeParameters();
            _texts = new(texts);
            RenderTextLines(_texts);
            _state = SceneState.CrossFading;
        }

        private void RenderTextLines(TextData texts)
        {
            DisposeTextLines();

            var str = texts.Texts[texts.TextIndex].ReplaceLineEndings("\n");
            var lines = str.Split('\n');
            _textLines = new UiText[lines.Length];
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                _textLines[i] = new UiTextOutlined(UiFonts.SceneText, line);
                AddChild(_textLines[i]);
            }

            UpdateTextPosition();
        }

        private void DisposeTextLines()
        {
            foreach (var text in _textLines)
            {
                text.Dispose();
                RemoveChild(text);
            }
            _textLines = new UiText[0];
        }

        private void UpdateTextPosition()
        {
            for (var i = 0; i < _textLines.Length; i++)
            {
                var line = _textLines[i];
                line.SetPreferredSize();
                var x = Width / 2 - line.Width / 2;
                var y = LetterboxMargin + 28 + (9 - _textLines.Length / 2 + i) * 20;
                line.SetPosition(x, y);
            }
        }

        public void ShowDialog(IList<SceneDialogText> texts)
        {
            if (texts.Count == 0)
            {
                Logger.ErrorS("scene", $"No dialog texts to display!");
                _state = SceneState.Advancing;
                return;
            }

            if (!_shownAnything)
            {
                SetCrossFadeParameters();
                _state = SceneState.CrossFadingToDialog;
            }
            else
            {
                _state = SceneState.Dialog;
            }
            _dialog = texts;
        }

        private void StartDialog(IList<SceneDialogText> texts)
        {
            DisposeTextLines();

            _shownAnything = true;
            _state = SceneState.ShowingDialog;

            var args = new DialogArgs();
            var dialogLayer = UserInterfaceManager.CreateLayer<DialogLayer, DialogArgs, DialogResult>(args);
            foreach (var text in texts)
            {
                string speakerName;
                PrototypeId<PortraitPrototype>? portraitID = null;
                if (_actors.TryGetValue(text.Actor, out var actor))
                {
                    if (Loc.TryGetString(actor.Name, out var name))
                        speakerName = name;
                    else
                        speakerName = (string)actor.Name;
                    portraitID = actor.PortraitID;
                }
                else
                {
                    Logger.ErrorS("scene", $"Actor {text.Actor} not found!");
                    speakerName = "????";
                }

                var step = new DialogStepData()
                {
                    PortraitID = portraitID,
                    Target = null,
                    SpeakerName = speakerName,
                    Text = text.Text,
                    Choices = new List<DialogChoice>()
                    {
                        new DialogChoice()
                        {
                            Text = Loc.GetString("Elona.Dialog.Common.Choices.More"),
                        }
                    },
                    CanCancel = true
                };

                dialogLayer.UpdateFromStepData(step);

                // BUG: reentrancy causes infinite loop
                var queryLayerArgs = new QueryLayerArgs(/* NoHaltInput: true */);

                var result = UserInterfaceManager.Query(dialogLayer, queryLayerArgs);
                if (!result.HasValue)
                {
                    Logger.WarningS("scene", "TODO cancel scene");
                }

                UpdateCrossFadeImage();
            }

            _dialog = null;
            _state = SceneState.Advancing;
        }

        public void Wait()
        {
            _state = SceneState.Waiting;
            _waitTimer = 1f;
        }

        public void FadeOut()
        {
            _state = SceneState.FadingOut;
            _waitTimer = 1.5f;
            _crossFadeStart = _waitTimer;
        }

        public void FadeIn()
        {
            _state = SceneState.FadingIn;
            _waitTimer = 0.75f;
            _crossFadeStart = _waitTimer;
        }

        private void AdvanceState()
        {
            if (_texts != null)
            {
                _texts.TextIndex++;
                if (_texts.TextIndex >= _texts.Texts.Count)
                {
                    _state = SceneState.Advancing;
                    _texts = null;
                }
                else
                {
                    RenderTextLines(_texts);
                    SetCrossFadeParameters();
                    _state = SceneState.CrossFading;
                    return;
                }
            }

            if (CurrentNode == null)
            {
                _waitTimer = 1.5f;
                _crossFadeStart = _waitTimer;
                _state = SceneState.Ending;
                return;
            }

            DisposeTextLines();

            while (_state == SceneState.Advancing && CurrentNode != null)
            {
                CurrentNode.OnEnter(this);
                currentNodeIndex++;
            }
        }

        private SceneState AdvanceStateFromTimer()
        {
            switch (_state)
            {
                case SceneState.CrossFading:
                    return SceneState.AwaitingInput;
                case SceneState.CrossFadingToDialog:
                    return SceneState.Dialog;
                case SceneState.Ending:
                    Finish(new());
                    return SceneState.Done;
                default:
                    return SceneState.Advancing;
            }
        }

        public override void Update(float dt)
        {
            _morePrompt.Update(dt);

            if (_state == SceneState.InputSent)
            {
                if (_morePrompt.FinishedClosing)
                {
                    UpdateCrossFadeImage();
                    _state = SceneState.Advancing;
                }
                else
                    return;
            }

            if (_canvasRebake)
            {
                _canvasRebake = false;
            }

            if (_waitTimer > 0f)
            {
                _waitTimer -= dt;
                if (_waitTimer <= 0)
                {
                    _morePrompt.IsPromptHidden = false;
                    _state = AdvanceStateFromTimer();
                    UpdateCrossFadeImage();
                }
            }

            if (_state == SceneState.Dialog)
            {
                StartDialog(_dialog!);
            }

            if (_state == SceneState.Advancing)
            {
                AdvanceState();
            }
        }

        private const int LetterboxMargin = 60;

        public override void Draw()
        {
            Love.Graphics.Clear(Color.Black);
            _background.DrawUnscaled(0, LetterboxMargin * UIScale, PixelWidth, PixelHeight - (LetterboxMargin * 2 * UIScale));

            Love.Graphics.SetColor(Color.White.WithAlphaB(70));
            Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);

            for (var i = 0; i < _textLines.Length; i++)
            {
                var y = LetterboxMargin + 31 + (9 - _textLines.Length / 2 + i) * 20 + 4;
                var shadowWidth = 80 + _textLines[i].Width;
                if (shadowWidth < 180)
                    shadowWidth = 0;
                _assetSceneTextShadow.Draw(UIScale, Width / 2, y, shadowWidth, null, centered: true);
            }

            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);

            if (_texts != null)
            {
                foreach (var text in _textLines)
                {
                    text.Draw();
                }
            }

            if (_crossFadeStart > 0f && _waitTimer > 0f)
            {
                var perc = ((_crossFadeStart - _waitTimer) / _crossFadeStart);
                if ((_state == SceneState.CrossFading || _state == SceneState.CrossFadingToDialog) && _crossFadeImage != null)
                {
                    if (_waitTimer > 0f)
                    {
                        Love.Graphics.SetColor(Color.White.WithAlphaF(1f - perc));
                        Love.Graphics.Draw(_crossFadeImage, 0, 0, 0, PixelWidth / _crossFadeImage.GetWidth(), PixelHeight / _crossFadeImage.GetHeight());
                    }
                }
                else if ((_state == SceneState.FadingOut || _state == SceneState.Ending) && _crossFadeImage != null)
                {
                    Love.Graphics.SetColor(Color.White);
                    Love.Graphics.Draw(_crossFadeImage, 0, 0, 0, PixelWidth / _crossFadeImage.GetWidth(), PixelHeight / _crossFadeImage.GetHeight());

                    Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
                    Love.Graphics.SetColor(Color.White.WithAlphaF(perc));
                    Love.Graphics.Rectangle(Love.DrawMode.Fill, 0, 0, PixelWidth, PixelHeight);
                    Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
                }
                else if (_state == SceneState.FadingIn)
                {
                    Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
                    Love.Graphics.SetColor(Color.White.WithAlphaF(1f - perc));
                    Love.Graphics.Rectangle(Love.DrawMode.Fill, 0, 0, PixelWidth, PixelHeight);
                    Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
                }
            }

            if (_state != SceneState.Dialog
                && _state != SceneState.ShowingDialog
                && _state != SceneState.FadingOut
                && _state != SceneState.FadingIn
                && _state != SceneState.Waiting
                && _state != SceneState.Ending
                && _state != SceneState.Done)
                _morePrompt.Draw();
        }
    }
}
