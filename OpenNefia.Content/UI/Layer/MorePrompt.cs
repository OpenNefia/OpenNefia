using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Logic
{
    public sealed class MorePrompt : UiLayerWithResult<UINone, UINone>
    {
        [Dependency] private readonly IGraphics _graphics = default!;

        private IAssetInstance AssetMorePrompt;

        public override int? DefaultZOrder => HudLayer.HudZOrder + 10000;

        private bool _canFinish = false;
        private float _size;
        private float _delay;
        private bool _closed = false;

        public bool IsPromptHidden { get; set; } = false;
        public bool IsClosing => HasResult || IsPromptHidden;
        public bool FinishedClosing => _closed;

        public MorePrompt()
        {
            AssetMorePrompt = Assets.Get(Asset.MorePrompt);

            CanControlFocus = true;
            OnKeyBindDown += HandleKeyBindDown;
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel || args.Function == EngineKeyFunctions.UISelect)
            {
                if (_canFinish)
                {
                    _size = Height / 2;
                    Sounds.Play(Sound.Ok1);
                    Finish(new());
                }
            }
        }

        public override void Initialize(UINone args)
        {
            _closed = false;
            _canFinish = false;
            _size = 0f;
            _delay = 0f;
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            var pos = _graphics.WindowSize - AssetMorePrompt.VirtualSize(UIScale);
            bounds = UIBox2.FromDimensions(pos, AssetMorePrompt.VirtualSize(UIScale));
        }

        public override UiResult<UINone>? GetResult()
        {
            var result = base.GetResult();

            if (result == null)
                return null;

            if ((HasResult && _size > 0f))
                return null;

            return result;
        }

        public override void Update(float dt)
        {
            var delta = dt * 50f;

            if (IsClosing)
            {
                _closed = _size <= 0f;
                // Retract the prompt momentarily before popping the layer.
                _size = float.Max(_size - delta * 2, 0f);
            }
            else
            {
                _closed = false;

                // Unfold the prompt and delay the player's input for a
                // short period so they can notice it.
                _size += delta;

                if (_size >= Height / 2)
                {
                    _size = Height / 2;
                    _delay += delta;

                    if (_delay > 1f)
                    {
                        _delay = 1f;
                        _canFinish = true;
                    }
                }
            }
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Color.White);
            if (IsClosing)
            {
                // FIXME don't use 0 as indicating default height...
                if (_size > 0f)
                    AssetMorePrompt.Draw(UIScale, X, Y + (Height / 2) - _size, Width, _size * 2f + 1f);
            }
            else if (_size > 0f)
            {
                AssetMorePrompt.Draw(UIScale, X, Y + (Height / 2) - _size, Width, _size * 2f + 1f);
            }
        }
    }
}