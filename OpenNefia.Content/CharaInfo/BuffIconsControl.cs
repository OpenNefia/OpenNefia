using OpenNefia.Content.Inventory;
using OpenNefia.Content.PCCs;
using OpenNefia.Content.Portraits;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Content.UI.Element.Containers;
using static OpenNefia.Content.CharaInfo.CharaSheetControl;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.Buffs;
using OpenNefia.Content.UI;
using OpenNefia.Core.Locale;
using OpenNefia.Content.DisplayName;
using OpenNefia.Core.Audio;

namespace OpenNefia.Content.CharaInfo
{
    public sealed class BuffIconsControl : UiElement
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly IBuffSystem _buffs = default!;
        [Dependency] private readonly IAssetManager _assets = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly IAudioManager _audio = default!;

        public class HexAndBlessingIcon : UiElement
        {
            private const int TileWidth = 32;
            private const int TileHeight = 32;

            private IAssetInstance? HexBlessingIcon;
            private IAssetInstance TileIcon;

            public HexAndBlessingIcon(IAssetInstance? icon)
            {
                TileIcon = Assets.Get(Protos.Asset.BuffIconNone);
                HexBlessingIcon = icon;
            }

            public override void GetPreferredSize(out Vector2 size)
            {
                size.X = TileWidth;
                size.Y = TileHeight;
            }

            public override void Draw()
            {
                base.Draw();

                var x = X + TileWidth / 2;
                var y = Y + TileHeight / 2;

                if (HexBlessingIcon != null)
                {
                    GraphicsEx.SetColor(Color.White);
                    HexBlessingIcon.Draw(UIScale, x, y, centered: true);
                }
                else
                {
                    GraphicsEx.SetColor(255, 255, 255, 120);
                    TileIcon.Draw(UIScale, x, y, centered: true);
                }
            }
        }

        private EntityUid _entity;

        [Child] private UiContainer Container = new UiGridContainer(GridType.Horizontal, 5, xCentered: false, xSpace: 8);

        private List<BuffComponent> _buffList = new();
        private int _selected = 0;

        public BuffIconsControl()
        {
            EntitySystem.InjectDependencies(this);
        }

        public void RefreshFromEntity(EntityUid entity)
        {
            _entity = entity;

            Container.Clear();

            _buffList = _buffs.EnumerateBuffs(_entity).Take(15).ToList();
            for (int i = 0; i < 15; i++)
            {
                if (_buffList.Count > i)
                {
                    var asset = _assets.GetAsset(_buffList[i].Icon);
                    Container.AddElement(new HexAndBlessingIcon(asset));
                }
                else
                {
                    Container.AddElement(new HexAndBlessingIcon(null));
                }
            }
        }

        public string GetSelectedBuffDescription()
        {
            // >>>>>>>> elona122/shade2/command.hsp:2610 		if cs_buffMax!0{ ...
            if (_selected < 0 || _selected >= _buffList.Count)
                return Loc.GetString("Elona.CharaSheet.Group.BlessingHex.NotAffected");

            var buff = _buffList[_selected];
            var buffName = _displayNames.GetDisplayName(buff.Owner);
            var buffDesc = _buffs.LocalizeBuffDescription(buff.Owner, buff);

            return Loc.GetString("Elona.CharaSheet.Group.BlessingHex.HintBody",
                ("buffName", buffName), ("buffDesc", buffDesc), ("turns", buff.TurnsRemaining));
            // <<<<<<<< elona122/shade2/command.hsp:2614 			} ...
        }

        public void SelectNext()
        {
            if (_buffList.Count == 0)
                return;

            Sounds.Play(Protos.Sound.Cursor1);
            _selected = MathHelper.Wrap(_selected + 1, 0, _buffList.Count - 1);
        }

        public void SelectPrevious()
        {
            if (_buffList.Count == 0)
                return;

            Sounds.Play(Protos.Sound.Cursor1);
            _selected = MathHelper.Wrap(_selected - 1, 0, _buffList.Count - 1);
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            size = new(32, 32);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            Container.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            Container.SetPosition(X, Y);
        }

        public override void Update(float dt)
        {
            Container.Update(dt);
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Color.White);
            Container.Draw();

            if (_selected >= 0 && _selected < _buffList.Count)
            {
                var elem = Container.Children[_selected];
                Love.Graphics.SetColor(UiColors.CharaSheetSelectedBuff);
                Love.Graphics.Rectangle(Love.DrawMode.Fill, elem.GlobalPixelRect);
            }
        }
    }
}
