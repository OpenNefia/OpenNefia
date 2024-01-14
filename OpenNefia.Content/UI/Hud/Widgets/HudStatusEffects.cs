using OpenNefia.Content.Hud;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.World;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Hud
{
    public sealed class StatusIndicator
    {
        public string Text { get; set; } = string.Empty;
        public Color Color { get; set; } = UiColors.TextBlack;
    }

    public class HudStatusIndicators : BaseHudWidget
    {
        [Dependency] private readonly IEntityManager _entityMan = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        private readonly List<StatusIndicator> _indicators = new();
        private IAssetInstance _assetStatusEffectBar = default!;
        private int _maxWidth;
        
        private GetStatusIndicatorsEvent _evInstance = new();

        public override void Initialize()
        {
            _assetStatusEffectBar = Assets.Get(Protos.Asset.StatusEffectBar);
        }

        private void CalcMaxWidth()
        {
            _maxWidth = 50;
            foreach (var indicator in _indicators)
            {
                _maxWidth = Math.Max(_maxWidth, UiFonts.StatusIndicatorText.LoveFont.GetWidth(indicator.Text) + 20);
            }
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            CalcMaxWidth();
            size = new(_maxWidth, _indicators.Count * 20);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
        }
        
        public override void SetSize(float w, float h)
        {
            base.SetSize(w, h);
        }

        public override void RefreshWidget()
        {
            _indicators.Clear();

            _evInstance.OutIndicators.Clear();
            _entityMan.EventBus.RaiseEvent(_gameSession.Player, _evInstance);
            _indicators.AddRange(_evInstance.OutIndicators);

            SetPreferredSize();
        }

        public override void Draw()
        {
            GraphicsEx.SetFont(UiFonts.StatusIndicatorText);
            var y = Y;
            
            foreach (var indicator in _indicators)
            {
                Love.Graphics.SetColor(Color.White);
                _assetStatusEffectBar.Draw(UIScale, X, y, _maxWidth);

                Love.Graphics.SetColor(indicator.Color);
                GraphicsS.PrintS(UIScale, indicator.Text, X + 6, y); // y + vfix + 1
                y += 20;
            }
        }
    }

    public sealed class GetStatusIndicatorsEvent : EntityEventArgs
    {
        public List<StatusIndicator> OutIndicators { get; } = new();
    }
}
