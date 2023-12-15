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

namespace OpenNefia.Content.Scene
{
    public sealed class SceneLayer : UiLayerWithResult<SceneLayer.Args, UINone>, ISceneEngine
    {
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IGraphics _graphics = default!;

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
            Dialog,
            ShowingDialog,
            FadingOut,
            FadingIn,
            Waiting,
            CrossFading,
            Ending,
            Done
        }

        private record class DialogData(string ActorID, IList<string> Texts);

        private SceneState _state = SceneState.Advancing;
        private Dictionary<string, ActorSpec> _actors = new();
        private IAssetInstance _background = default!;

        [Child] private MorePrompt _morePrompt = new();

        private DialogData? _dialog = null;

        private float _waitTimer = 0f;
        private float _crossFadeStart = 0f;
        private bool _canvasRebake = false;
        private Love.Image? _crossFadeImage;

        private IList<ISceneNode> _nodes = new List<ISceneNode>();
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
            if (args.Function == EngineKeyFunctions.UISelect)
            {
                _audio.Play(Protos.Sound.Ok1);
                currentNodeIndex++;
                _state = SceneState.Advancing;
                args.Handle();
            }
            else if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
                args.Handle();
            }
        }

        private void StartCrossFade(float time = 0.25f)
        {
            _state = SceneState.CrossFading;
            _waitTimer = time;
            _crossFadeStart = _waitTimer;
        }

        public override void Initialize(Args args)
        {
            _nodes = args.Nodes;
            _background = Assets.Get(Protos.Asset.Bg1);
        }

        public override void OnQuery()
        {
            currentNodeIndex = 0;
            StartCrossFade();
            var data = _graphics.CaptureCanvasImageData();
            if (data != null)
            {
                _crossFadeImage = Love.Graphics.NewImage(data);
            }
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
        }

        public void SetActors(IDictionary<string, ActorSpec> actors)
        {
            _actors = new Dictionary<string, ActorSpec>(actors);
        }

        public void SetBackground(PrototypeId<AssetPrototype> assetID)
        {
            StartCrossFade();
        }

        public void ShowText(IList<string> text)
        {
            StartCrossFade();
            Logger.WarningS("scene", text[0]);
        }

        public void ShowDialog(string actorID, IList<string> texts)
        {
            _state = SceneState.Dialog;
            _dialog = new(actorID, texts);
        }

        private void StartDialog(DialogData dialog)
        {
            _state = SceneState.ShowingDialog;

            var actorID = dialog.ActorID;
            var texts = dialog.Texts;

            string speakerName;
            if (_actors.TryGetValue(actorID, out var actor))
            {
                speakerName = Loc.GetString(actor.Name);
            }
            else
            {
                Logger.ErrorS("scene", $"Actor {actorID} not found!");
                speakerName = Loc.GetString("Elona.Scene.Common.ActorName.Unknown");
            }

            var args = new DialogArgs();
            var dialogLayer = UserInterfaceManager.CreateLayer<DialogLayer, DialogArgs, DialogResult>(args);
            foreach (var text in texts)
            {
                var step = new DialogStepData()
                {
                    Target = null,
                    SpeakerName = speakerName,
                    Text = text,
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
            _waitTimer = 1f;
        }

        public void FadeIn()
        {
            _state = SceneState.FadingIn;
            _waitTimer = 1f;
        }

        private void AdvanceState()
        {
            if (CurrentNode == null)
            {
                _state = SceneState.Done;
                Finish(new());
                return;
            }

            while (_state == SceneState.Advancing && CurrentNode != null)
            {
                CurrentNode.OnEnter(this);
                currentNodeIndex++;
            }

            if (CurrentNode == null)
            {
                _state = SceneState.Done;
                Finish(new());
                return;
            }
        }

        private SceneState AdvanceStateFromTimer()
        {
            switch (_state)
            {
                case SceneState.CrossFading:
                    return SceneState.AwaitingInput;
                default:
                    return SceneState.Advancing;
            }
        }

        public override void Update(float dt)
        {
            if (_canvasRebake)
            {
                _canvasRebake = false;
            }

            if (_waitTimer > 0f)
            {
                _waitTimer -= dt;
                if (_waitTimer <= 0)
                {
                    _state = AdvanceStateFromTimer();
                    var data = _graphics.CaptureCanvasImageData();
                    if (data != null)
                    {
                        _crossFadeImage = Love.Graphics.NewImage(data);
                    }
                }
            }

            if (_state == SceneState.Advancing)
            {
                AdvanceState();
            }

            switch (_state)
            {
                case SceneState.AwaitingInput:
                default:
                    break;
                case SceneState.Dialog:
                    StartDialog(_dialog!);
                    break;
                case SceneState.FadingOut:
                    break;
                case SceneState.FadingIn:
                    break;
                case SceneState.Waiting:
                    break;
                case SceneState.CrossFading:
                    break;
                case SceneState.Ending:
                    break;
            }
        }

        public override void Draw()
        {
            _background.DrawUnscaled(0, 0, PixelWidth, PixelHeight);
            if (_crossFadeImage != null && _crossFadeStart > 0f)
            {
                if (_waitTimer <= 0f)
                {
                }
                else
                {
                    var alpha = 1f - ((_crossFadeStart - _waitTimer) / _crossFadeStart);
                    Console.WriteLine($"{alpha}");
                    Love.Graphics.SetColor(Color.White.WithAlphaF(alpha));
                    Love.Graphics.Draw(_crossFadeImage, 0, 0, 0, PixelWidth / _crossFadeImage.GetWidth(), PixelHeight / _crossFadeImage.GetHeight());
                }
            }

            Love.Graphics.SetColor(Color.White);

            switch (_state)
            {
                case SceneState.AwaitingInput:
                default:
                    break;
                case SceneState.Dialog:
                    break;
                case SceneState.FadingOut:
                    break;
                case SceneState.FadingIn:
                    break;
                case SceneState.Waiting:
                    break;
                case SceneState.CrossFading:
                    break;
                case SceneState.Ending:
                    break;
            }
        }
    }
}
