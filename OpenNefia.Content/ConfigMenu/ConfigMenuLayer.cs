using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Input;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
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
        }

        [Localize("Topic.Menu")]
        private IUiText TextTopicMenu = new UiTextTopic();

        private IAssetInstance AssetTitle;
        private IAssetDrawable AssetG2;

        private UiPagedList<bool> List;
        private UiWindow Window = new();

        private bool _isInTitleScreen;

        public ConfigMenuLayer()
        {
            List = new UiPagedList<bool>(elementForPageText: Window);
            AssetTitle = Assets.Get(Asset.Title);
            AssetG2 = new AssetDrawable(Asset.G2, color: new(255, 255, 255, 50));

            OnKeyBindDown += HandleKeyBindDown;
            CanControlFocus = true;

            List.EventOnActivate += HandleListActivate;

            AddChild(List);
        }

        public override void OnFocused()
        {
            base.OnFocused();
            List.GrabFocus();
        }

        public override void Initialize(Args args)
        {
            for (int i = 0; i < 5; i++)
            {
                List.Add(new ConfigItemBooleanCell(true));
            }

            Window.Title = "ConfigMenuName";
            Window.KeyHints = MakeKeyHints();
        }

        private void HandleListActivate(object? sender, UiListEventArgs<bool> evt)
        {
            Sounds.Play(Sound.Ok1);
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs obj)
        {
            if (obj.Function == EngineKeyFunctions.UICancel)
            {
                Finish(new());
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
            var menuWidth = 440;
            var menuHeight = 300;
            UiUtils.GetCenteredParams(menuWidth, menuHeight, out bounds);
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
