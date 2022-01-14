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

        private bool _canFinish = false;
        private bool _finished = false;
        private float _size;
        private float _size2;
        private float _delay;

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
                    Sounds.Play(Sound.Ok1);
                    Finish(new());
                }
            }
        }

        public override void Initialize(UINone args)
        {
            _canFinish = false;
            _finished = false;
            _size = 0f;
            _size2 = (AssetMorePrompt.Height / 2);
            _delay = 0f;
        }

        public override void GetPreferredBounds(out UIBox2i bounds)
        {
            var pos = _graphics.WindowSize - AssetMorePrompt.Size;
            bounds = UIBox2i.FromDimensions(pos, AssetMorePrompt.Size);
        }

        public override UiResult<UINone>? GetResult()
        {
            var result = base.GetResult();

            if (result == null)
                return null;

            if ((HasResult && _size2 >= 0f))
                return null;

            return result;
        }

        public override void Update(float dt)
        {
            var delta = dt * 50f;

            if (HasResult)
            {
                // Retract the prompt momentarily before popping the layer.
                _size2 -= delta * 2;

                if (_size2 < 0f)
                {
                    _finished = true;
                }
            }
            else
            {
                // Unfold the prompt and delay the player's input for a
                // short period so they can notice it.
                _size += delta;

                if (_size >= (AssetMorePrompt.Height / 2))
                {
                    _size = (AssetMorePrompt.Height / 2);
                    _delay += delta;

                    if (_delay > 20f)
                    {
                        _delay = 20f;
                        _canFinish = true;
                    }
                }
            }
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Color.White);

            if (HasResult)
            {
                // FIXME don't use 0 as indicating default height...
                AssetMorePrompt.Draw(X, Y + (AssetMorePrompt.Height / 2) - _size2, Width, _size2 * 2f + 1f);
            }
            else if (_size > 0f)
            {
                AssetMorePrompt.Draw(X, Y + (AssetMorePrompt.Height / 2) - _size, Width, _size * 2f + 1f);
            }
        }
    }
}