using OpenNefia.Content.ConfigMenu.UICell;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.ConfigMenu
{
    [Localize("Elona.Config.Layer")]
    public sealed class ConfigMenuLayer : UiLayerWithResult<ConfigMenuLayer.Args, UINone>
    {
        public class Args
        {
            public PrototypeId<ConfigMenuItemPrototype> PrototypeId { get; }
            public ConfigSubmenuMenuNode Submenu { get; }

            public Args(PrototypeId<ConfigMenuItemPrototype> prototypeId, ConfigSubmenuMenuNode submenu)
            {
                PrototypeId = prototypeId;
                Submenu = submenu;
            }
        }

        [Dependency] private readonly IConfigMenuUICellFactory _cellFactory = default!;

        [Localize("Topic.Menu")]
        private IUiText TextTopicMenu = new UiTextTopic();

        private IAssetDrawable AssetG2;

        // The UI cells are generic based on the config option type, so UINone is
        // used to have them all in one list.
        // FIXME: #35
        private UiPagedList<UINone> List;
        private UiWindow Window = new();

        private Vector2i _menuSize = new();

        public ConfigMenuLayer()
        {
            List = new UiPagedList<UINone>(elementForPageText: Window);
            AssetG2 = new AssetDrawable(Asset.G2, color: new(255, 255, 255, 50));

            OnKeyBindDown += HandleKeyBindDown;
            List.OnActivated += HandleListActivate;
            List.OnPageChanged += HandleListPageChanged;

            AddChild(List);
        }

        public override void OnFocused()
        {
            base.OnFocused();
            List.GrabFocus();
        }

        public override void Initialize(Args args)
        {
            foreach (var child in args.Submenu.Items)
            {
                List.Add(_cellFactory.CreateUICellFor(child));
            }

            _menuSize = args.Submenu.MenuSize;

            Window.Title = Loc.GetPrototypeString(args.PrototypeId, "Name");
            Window.KeyHints = MakeKeyHints();

            RefreshConfigValueDisplay();
        }

        private void RefreshConfigValueDisplay()
        {
            // FIXME: #35
            foreach (var cell in List.Cast<BaseConfigMenuUICell>())
            {
                cell.RefreshConfigValueDisplay();
            }
        }

        private void HandleListActivate(object? sender, UiListEventArgs<UINone> evt)
        {
            Sounds.Play(Sound.Ok1);
            ((BaseConfigMenuUICell)evt.SelectedCell).HandleActivated();
            RefreshConfigValueDisplay();
        }

        private void HandleListPageChanged(int newPage, int newPageCount)
        {
            // Recenter the window based on the new item count.
            SetPreferredSize();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs evt)
        {
            if (evt.Function == EngineKeyFunctions.UICancel)
            {
                Finish(new());
            }

            if (List.SelectedCell is not BaseConfigMenuUICell selected)
                return;

            if (evt.Function == EngineKeyFunctions.UILeft)
            {
                selected.HandleChanged(-1);
            }
            else if (evt.Function == EngineKeyFunctions.UIRight)
            {
                selected.HandleChanged(1);
            }
            else if (evt.Function == EngineKeyFunctions.UISelect)
            {
                selected.HandleActivated();
            }
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            keyHints.AddRange(List.MakeKeyHints());
            keyHints.Add(new(UiKeyHints.Close, EngineKeyFunctions.UICancel));

            return keyHints;
        }

        public override void GetPreferredBounds(out UIBox2i bounds)
        {
            var height = _menuSize.Y;

            if (List.DisplayedCells.Count >= 9)
            {
                height += 10 + 30 * (List.DisplayedCells.Count - 9);
            }

            UiUtils.GetCenteredParams(_menuSize.X, height, out bounds);
            bounds.Top -= 12;
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Window.SetSize(width, height);
            TextTopicMenu.SetPreferredSize();
            AssetG2.SetSize(Window.Width / 5 * 3, Window.Height - 80);
            List.SetSize(Window.Width - 56, Window.Height - 66);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Window.SetPosition(X, Y);
            TextTopicMenu.SetPosition(Window.X + 34, Window.Y + 36);
            AssetG2.SetPosition(Window.X + Window.Width / 3, Window.Y + Window.Height / 2);
            AssetG2.Centered = true;
            List.SetPosition(Window.X + 56, Window.Y + 66);
        }

        public override void Update(float dt)
        {
            Window.Update(dt);
            TextTopicMenu.Update(dt);
            AssetG2.Update(dt);
            List.Update(dt);
        }

        public override void Draw()
        {
            Window.Draw();
            TextTopicMenu.Draw();
            AssetG2.Draw();
            List.Draw();
        }
    }
}
