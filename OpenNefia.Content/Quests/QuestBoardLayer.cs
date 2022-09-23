using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Audio;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Content.UI;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.Input;
using Love;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Utility;
using OpenNefia.Content.UI.Element;
using Color = OpenNefia.Core.Maths.Color;
using OpenNefia.Content.Levels;
using OpenNefia.Core.Game;
using OpenNefia.Content.Quests;

namespace OpenNefia.Content.Quests
{
    public sealed class QuestBoardList : UiPagedList<QuestBoardList.Item>
    {
        public sealed class Item
        {
            public QuestComponent Quest { get; }
            public string QuestName { get; }
            public string QuestDescription { get; }
            public string DeadlineText { get; }
            public Color DifficultyColor { get; }

            public Item(QuestComponent quest, string name, string description, string deadlineText, Color difficultyColor)
            {
                Quest = quest;
                QuestName = name;
                QuestDescription = description;
                DeadlineText = deadlineText;
                DifficultyColor = difficultyColor;
            }
        }

        public sealed class QuestBoardListCell : UiListCell<Item>
        {
            [Child] private UiWindow Window = new() { ShowDecorations = false };
            [Child] private AssetDrawable AssetDecoBoardB;
            [Child] private UiText TextDeadline = new(UiFonts.ListText);
            [Child] private UiText TextClientName = new(UiFonts.ListText);
            [Child] private UiText TextStarsRow1 = new(UiFonts.QuestBoardDifficultyNormal);
            [Child] private UiText TextStarsRow2 = new(UiFonts.QuestBoardDifficultyNormal);
            [Child] private UiWrappedText TextDescription = new(UiFonts.ListText);

            private int _starCount;

            public QuestBoardListCell(Item data, UiListChoiceKey? key = null) : base(data, new UiText(UiFonts.ListText), key)
            {
                AssetDecoBoardB = new AssetDrawable(Protos.Asset.DecoBoardB);

                var difficulty = Data.Quest.Difficulty;
                _starCount = difficulty / 5 + 1;

                UiText.Text = Data.QuestName;
                TextDeadline.Text = Data.DeadlineText;
                TextClientName.Text = Data.Quest.ClientName;

                if (_starCount <= 5)
                {
                    TextStarsRow1.Text = Loc.GetString("Elona.Quest.Board.Difficulty.Star").Repeat(_starCount);
                }
                else if (_starCount <= 10)
                {
                    TextStarsRow1.Text = Loc.GetString("Elona.Quest.Board.Difficulty.Star").Repeat(5);
                    TextStarsRow2.Text = Loc.GetString("Elona.Quest.Board.Difficulty.Star").Repeat(_starCount - 5);

                    TextStarsRow1.Font = UiFonts.QuestBoardDifficultySmall;
                    TextStarsRow2.Font = UiFonts.QuestBoardDifficultySmall;
                }
                else
                {
                    TextStarsRow1.Text = Loc.GetString("Elona.Quest.Board.Difficulty.Counter", ("stars", _starCount));
                }
                TextStarsRow1.Color = Data.DifficultyColor;
                TextStarsRow2.Color = Data.DifficultyColor;

                TextDescription.WrappedText = Data.QuestDescription;
            }

            public override void SetPosition(float x, float y)
            {
                base.SetPosition(x, y);
                Window.SetPosition(X, Y);
                AssetDecoBoardB.SetPosition(X + 20, Y + 8);
                UiText.SetPosition(X + 76, y - 1);
                TextDeadline.SetPosition(X + 324, Y + 2);
                TextClientName.SetPosition(X + 372, Y + 2);

                if (_starCount <= 5)
                {
                    TextStarsRow1.SetPosition(X + 250, Y + 2);
                }
                else if (_starCount < 11)
                {
                    TextStarsRow1.SetPosition(X + 250, Y - 1);
                    TextStarsRow2.SetPosition(X + 250, Y + 7);
                }
                else
                {
                    TextStarsRow1.SetPosition(X + 250, Y + 2);
                }

                TextDescription.SetPosition(X, Y + 20);
            }

            public override void SetSize(float width, float height)
            {
                base.SetSize(width, height);
                Window.SetSize(Width, Height);
                AssetDecoBoardB.SetPreferredSize();
                UiText.SetPreferredSize();
                TextDeadline.SetPreferredSize();
                TextClientName.SetPreferredSize();
                TextStarsRow1.SetPreferredSize();
                TextStarsRow2.SetPreferredSize();
                TextDescription.SetSize(Width, Height);
            }

            public override void Update(float dt)
            {
                Window.Update(dt);
                AssetDecoBoardB.Update(dt);
                UiText.Update(dt);
                TextDeadline.Update(dt);
                TextClientName.Update(dt);
                TextStarsRow1.Update(dt);
                TextStarsRow2.Update(dt);
                TextDescription.Update(dt);
            }

            public override void Draw()
            {
                Window.Draw();
                AssetDecoBoardB.Draw();
                AssetSelectKey.Draw(UIScale, X, Y - 1);
                UiText.Draw();
                TextDeadline.Draw();
                TextClientName.Draw();
                TextStarsRow1.Draw();
                TextStarsRow2.Draw();
                TextDescription.Draw();
            }
        }

        public MapCoordinates TileOrigin { get; set; }

        public QuestBoardList() : base(4)
        {
            EntitySystem.InjectDependencies(this);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);

            for (int index = 0; index < DisplayedCells.Count; index++)
            {
                var cell = DisplayedCells[index];

                cell.SetSize(Width, 120);
            }
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);

            var iy = Y;

            for (int index = 0; index < DisplayedCells.Count; index++)
            {
                var cell = DisplayedCells[index];


                cell.SetPosition(X, iy);
                iy += cell.Height;
            }
        }
    }

    public sealed class QuestBoardLayer : UiLayerWithResult<QuestBoardLayer.Args, QuestBoardLayer.Result>
    {
        public class Args
        {
            public Args(List<QuestComponent> quests)
            {
                Quests = quests;
            }

            public List<QuestComponent> Quests { get; }
        }

        public class Result
        {
            public Result(QuestComponent selectedQuest)
            {
                SelectedQuest = selectedQuest;
            }

            public QuestComponent SelectedQuest { get; }
        }

        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IQuestSystem _quests = default!;
        [Dependency] private readonly IGraphics _graphics = default!;

        private IAssetInstance AssetDecoBoardA;
        [Child] private QuestBoardList List = new();

        public QuestBoardLayer()
        {
            OnKeyBindDown += HandleKeyBindDown;
            EventFilter = UIEventFilterMode.Pass;

            AssetDecoBoardA = Assets.Get(Protos.Asset.DecoBoardA);

            List.OnActivated += List_OnActivated;
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs ev)
        {
            if (ev.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
            }
        }

        private void List_OnActivated(object? sender, UiListEventArgs<QuestBoardList.Item> e)
        {
            if (_playerQuery.YesOrNo(Loc.GetString("Elona.Quest.Board.PromptMeetClient")))
            {
                Finish(new Result(e.SelectedCell.Data.Quest));
            }
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            List.GrabFocus();
        }

        private static Color GetQuestDifficultyColor(int playerLevel, int difficulty)
        {
            if (playerLevel * 2 < difficulty)
                return UiColors.QuestDifficultyVeryHigh;
            else if ((int)(playerLevel * 1.5) < difficulty)
                return UiColors.QuestDifficultyHigh;
            else if (playerLevel < difficulty)
                return UiColors.QuestDifficultyModerate;
            else
                return UiColors.QuestDifficultyEasy;
        }

        public override void Initialize(Args args)
        {
            var playerLevel = _levels.GetLevel(_gameSession.Player);

            List.SetCells(args.Quests.Select(quest =>
            {
                var localized = _quests.LocalizeQuestData(quest.Owner, quest.ClientEntity, _gameSession.Player, quest);
                var deadlineText = _quests.FormatDeadlineText(quest.TimeUntilDeadline);
                var difficultyColor = GetQuestDifficultyColor(playerLevel, quest.Difficulty);
                var item = new QuestBoardList.Item(quest, localized.Name, localized.Description, deadlineText, difficultyColor);
                return new QuestBoardList.QuestBoardListCell(item);
            }));
        }

        public override void OnQuery()
        {
            _audio.Play(Protos.Sound.Chat);

            if (List.Count == 0)
            {
                _mes.Display(Loc.GetString("Elona.Quest.Board.NoNewNotices"));
                Cancel();
            }
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(560, 140 * 4, out bounds);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            List.SetSize(Width - 40, Height - 40);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            List.SetPosition(X + 20, Y + 20);
        }

        public override void Update(float dt)
        {
            List.Update(dt);
        }

        private void DrawTiled(IAssetInstance asset)
        {
            var iw = asset.VirtualWidth(UIScale);
            var ih = asset.VirtualHeight(UIScale);

            for (var j = 0; j < _graphics.WindowSize.Y / ih; j++)
                for (var i = 0; i < _graphics.WindowSize.X / iw; i++)
                    asset.Draw(UIScale, i * iw, j * ih);
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Color.White);

            DrawTiled(AssetDecoBoardA);

            List.Draw();
        }
    }
}
