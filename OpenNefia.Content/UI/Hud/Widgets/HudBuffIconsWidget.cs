using OpenNefia.Core.Rendering;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Content.DisplayName;
using OpenNefia.Core.UI;
using Love;
using OpenNefia.Content.Buffs;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Game;

namespace OpenNefia.Content.Hud
{
    public class HudBuffIconsWidget : BaseHudWidget
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private IDisplayNameSystem _nameSystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        [Child] private UiTextShadowed _textBuffDuration = new(UiFonts.HUDBuffDurationText);

        private BuffsComponent? _buffs = null;

        public HudBuffIconsWidget()
        {
        }

        public override void RefreshWidget()
        {
            base.RefreshWidget();
            _buffs = _entityManager.GetComponentOrNull<BuffsComponent>(_gameSession.Player);
            SetSize(Width, Height);
        }

        public override void SetSize(float width, float height)
        {
            width = float.Max(width, 32);
            height = 0;
            if (_buffs != null)
            {
                foreach (var buff in _buffs.Container.ContainedEntities)
                {
                    if (!_entityManager.TryGetComponent<BuffComponent>(buff, out var buffComp))
                        continue;

                    var icon = Assets.Get(buffComp.Icon);
                    width = float.Max(icon.VirtualWidth(UIScale), width);
                    height += icon.PixelHeight;
                }
            }

            base.SetSize(width, height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
        }

        public override void Draw()
        {
            // >>>>>>>> elona122/shade2/screen.hsp:335 	gmode 4,,,180 ...
            base.Draw();

            if (_buffs == null)
                return;

            var y = Y + Height;

            foreach (var buff in _buffs.Container.ContainedEntities)
            {
                if (!_entityManager.TryGetComponent<BuffComponent>(buff, out var buffComp))
                    continue;

                var icon = Assets.Get(buffComp.Icon);

                y -= icon.PixelHeight;

                Love.Graphics.SetColor(Core.Maths.Color.White.WithAlphaB(180));
                icon.Draw(UIScale, X, y);

                _textBuffDuration.Text = buffComp.TurnsRemaining.ToString();
                _textBuffDuration.SetPosition(X + 2, y + 18);
                _textBuffDuration.SetPreferredSize();
                _textBuffDuration.Draw();
            }
            // <<<<<<<< elona122/shade2/screen.hsp:343 	loop ...
        }
    }
}
