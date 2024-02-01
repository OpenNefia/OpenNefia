using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Input;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using OpenNefia.VisualAI.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.UI.Element.List;

namespace OpenNefia.VisualAI.UserInterface
{
    public sealed class VisualAIInsertMenu : UiLayerWithResult<VisualAIInsertMenu.Args, VisualAIInsertMenu.Result>
    {
        [Dependency] private readonly IAudioManager _audio = default!;

        public class Args
        {
            public string WindowTitle { get; set; } = string.Empty;
            public VisualAIBlockType Category { get; set; } = VisualAIBlockType.Target;
            public PrototypeId<VisualAIBlockPrototype>? BlockID { get; set; }
        }

        public class Result
        {
            public Result(VisualAIBlock block)
            {
                Block = block;
                LastCategory = block.Proto.Type;
            }

            public Result(VisualAIBlockType category)
            {
                LastCategory = category;
            }

            public VisualAIBlock? Block { get; }
            public VisualAIBlockType LastCategory { get; }
        }

        [Child] private UiWindow Window = new();
        [Child] private VisualAIBlockList List = new();
        [Child] private UiTopicWindow CategoryWindow = new();
        [Child] private UiTextOutlined CategoryText = new(new FontSpec(15, 15, color: UiColors.TextWhite, bgColor: UiColors.TextBlack));

        public VisualAIInsertMenu()
        {
            OnKeyBindDown += HandleKeyBindDown;
            List.OnActivated += HandleActivated;
            List.OnCategoryChanged += HandleCategoryChanged;
        }

        public override void Initialize(Args args)
        {
            if (args.BlockID != null)
                List.SelectBlock(args.BlockID.Value);
            else
                List.SetCategory(args.Category);

            Window.Title = args.WindowTitle;
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                Finish(new Result(List.SelectedCategory));
                args.Handle();
            }
        }

        private void HandleActivated(object? sender, UiListEventArgs<VisualAIBlockList.Item> e)
        {
            _audio.Play(Protos.Sound.Ok1);

            var block = new VisualAIBlock(e.SelectedCell.Data.BlockID);

            // TODO variable config

            Finish(new Result(block));
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            List.GrabFocus();
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();
            keyHints.AddRange(List.MakeKeyHints());
            keyHints.Add(new(UiKeyHints.Close, EngineKeyFunctions.UICancel));
            return keyHints;
        }

        public override void OnQuery()
        {
            _audio.Play(Protos.Sound.Pop2);
        }

        private void HandleCategoryChanged(VisualAIBlockType blockType)
        {
            CategoryText.Text = Loc.GetString($"VisualAI.UI.Editor.Block.Category.{blockType}");
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(400, 520, out bounds);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            Window.SetSize(Width, Height);
            CategoryWindow.SetSize(Width - 20 - 100, 40);
            CategoryText.SetPreferredSize();
            List.SetSize(Width - 20, Height - 20 - CategoryWindow.Height - 30);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            Window.SetPosition(X, Y);
            List.SetPosition(X + 15, Y + 10 + 50);
            CategoryWindow.SetPosition(X + 10 + 50, Y + 32);
            CategoryText.SetPosition(CategoryWindow.X + CategoryWindow.Width / 2 - CategoryText.Width / 2,
                                     CategoryWindow.Y + CategoryWindow.Height / 2 - CategoryText.Height / 2);
        }

        public override void Draw()
        {
            Window.Draw();
            List.Draw();
            CategoryWindow.Draw();
            CategoryText.Draw();
        }
    }
}
