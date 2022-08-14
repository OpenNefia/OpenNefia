using OpenNefia.Content.CharaMake;
using OpenNefia.Content.Logic;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core;
using OpenNefia.Core.Audio;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.Utility;
using System.Diagnostics.CodeAnalysis;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.TitleScreen
{
    [Localize("Elona.RestoreSave.Layer")]
    public sealed partial class RestoreSaveLayer : UiLayerWithResult<UINone, RestoreSaveLayer.Result>
    {
        private sealed class RestoreSaveCellData
        {
            public ISaveGameHandle SaveGame { get; }

            public RestoreSaveCellData(ISaveGameHandle saveGame)
            {
                SaveGame = saveGame;
            }
        }

        private sealed class RestoreSaveUICell : UiListCell<RestoreSaveCellData>
        {
            [Child] private UiText TextSaveDate = new UiText();

            public RestoreSaveUICell(RestoreSaveCellData data)
                : base(data, new UiText(), null)
            {
                OnCellDataChanged();
            }

            protected override void OnCellDataChanged()
            {
                UiText.Text = Data.SaveGame.Header.Name;
                TextSaveDate.Text = Data.SaveGame.LastWriteTime.ToString();
            }

            public override void GetPreferredSize(out Vector2 size)
            {
                base.GetPreferredSize(out size);
                TextSaveDate.GetPreferredSize(out var size2);
                size.X += size2.X + 55;
            }

            public override void SetSize(float width, float height)
            {
                base.SetSize(width, height);
                TextSaveDate.SetPreferredSize();
            }

            public override void SetPosition(float x, float y)
            {
                base.SetPosition(x, y);
                TextSaveDate.SetPosition(X + 155, Y);
            }

            public override void Update(float dt)
            {
                base.Update(dt);
                TextSaveDate.Update(dt);
            }

            public override void Draw()
            {
                base.Draw();
                TextSaveDate.Draw();
            }
        }

        public new class Result
        {
            public ISaveGameHandle SaveGame { get; }

            public Result(ISaveGameHandle saveGame)
            {
                SaveGame = saveGame;
            }
        }

        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;

        private readonly IAssetInstance AssetVoid;
        private readonly AssetDrawable AssetNoScreenshot;

        [Child][Localize] private readonly UiWindow Window = new();
        [Child][Localize] private readonly CharaMakeCaption Caption = new();
        [Child] private readonly UiPagedList<RestoreSaveCellData> List = new(itemsPerPage: 18);
        private readonly Dictionary<ISaveGameHandle, Love.Image> _imageCache = new();

        [Child] private readonly UiFittedBox ScreenshotBox;
        private UiTextureElement? ScreenshotElement;

        [Child][Localize("NoSaves")] private readonly UiText TextNoSaves = new UiText();
        [Child][Localize("Topic.SaveName")] private readonly UiText TextTopicSaveName = new UiTextTopic();
        [Child][Localize("Topic.SaveDate")] private readonly UiText TextTopicSaveDate = new UiTextTopic();

        public RestoreSaveLayer()
        {
            AssetVoid = Assets.Get(Asset.Void);
            AssetNoScreenshot = new AssetDrawable(Asset.NoScreenshot);

            OnKeyBindDown += HandleKeyBindDown;
            List.OnActivated += HandleListActivated;
            List.OnSelected += HandleListSelected;

            ScreenshotBox = new UiFittedBox()
            {
                Alignment = UiAlignment.Center,
                BoxFit = UiBoxFit.Contain
            };
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            List.GrabFocus();
        }

        public override void Initialize(UINone args)
        {
            RebuildList();
        }

        private void RebuildList()
        {
            _saveGameManager.RescanSaves();

            var cells = _saveGameManager.AllSaves
                .OrderByDescending(save => save.LastWriteTime)
                .Select(save => new RestoreSaveUICell(new RestoreSaveCellData(save)));

            List.SetCells(cells);

            Window.KeyHints = MakeKeyHints();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
            }
            else if (args.Function == EngineKeyFunctions.TextBackspace)
            {
                // FIXME: #35
                if (List.SelectedCell is RestoreSaveUICell cell)
                {
                    PromptDelete(cell);
                }
            }
        }

        private void HandleListSelected(object? sender, UiListEventArgs<RestoreSaveCellData> evt)
        {
            ScreenshotElement = null;

            var save = evt.SelectedCell.Data.SaveGame;

            if (TryGetScreenshotImage(save, out var image))
            {
                ScreenshotElement = new UiTextureElement(image);
                ScreenshotBox.Child = ScreenshotElement;
            }
            else
            {
                ScreenshotBox.Child = AssetNoScreenshot;
            }
        }

        private bool TryGetScreenshotImage(ISaveGameHandle save, [NotNullWhen(true)] out Love.Image? image)
        {
            if (_imageCache.TryGetValue(save, out image))
                return true;

            var screenshotPath = SaveGameConstants.ScreenshotPath;

            if (!save.Files.Exists(screenshotPath))
                return false;

            var fileData = save.Files.ReadAllLoveFileData(screenshotPath);
            var imageData = Love.Image.NewImageData(fileData);
            image = Love.Graphics.NewImage(imageData);

            _imageCache[save] = image;
            return true;
        }

        private void HandleListActivated(object? sender, UiListEventArgs<RestoreSaveCellData> evt)
        {
            Sounds.Play(Sound.Ok1);
            Finish(new(evt.SelectedCell.Data.SaveGame));
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            keyHints.AddRange(List.MakeKeyHints());

            keyHints.Add(new(UiKeyHints.Back, EngineKeyFunctions.UICancel));
            keyHints.Add(new(new LocaleKey("Elona.RestoreSave.Layer.KeyHint.Delete"), EngineKeyFunctions.TextBackspace));

            return keyHints;
        }

        private void PromptDelete(RestoreSaveUICell cell)
        {
            var captionText = Caption.Text;

            var save = cell.Data.SaveGame;
            var saveName = save.Header.Name;

            var opts = new YesOrNoOptions()
            {
                Invert = true,
                QueryText = Loc.GetString("Elona.RestoreSave.Layer.Delete.Confirm", ("saveName", saveName))
            };

            Caption.Text = opts.QueryText;

            if (_playerQuery.YesOrNo(opts))
            {
                opts.QueryText = Loc.GetString("Elona.RestoreSave.Layer.Delete.ConfirmFinal", ("saveName", saveName));
                Caption.Text = opts.QueryText;

                if (_playerQuery.YesOrNo(opts))
                {
                    _saveGameManager.DeleteSave(save);
                    RebuildList();
                }
            }

            Caption.Text = captionText;
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(680, 500, out bounds, yOffset: 20);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            Window.SetSize(Width, Height);
            Caption.SetPreferredSize();
            List.SetPreferredSize();
            TextNoSaves.SetPreferredSize();
            TextTopicSaveName.SetPreferredSize();
            TextTopicSaveDate.SetPreferredSize();
            ScreenshotBox.SetSize(300, 225);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            Window.SetPosition(X, Y);
            Caption.SetPosition(20, 30);
            List.SetPosition(Window.X + 40, Window.Y + 65);
            TextNoSaves.SetPosition(Window.X + Window.Width / 2 - TextNoSaves.Width / 2, Window.Y + Window.Height / 2 - TextNoSaves.Height);
            TextTopicSaveName.SetPosition(Window.X + 40, Window.Y + 35);
            TextTopicSaveDate.SetPosition(Window.X + 190, Window.Y + 35);
            ScreenshotBox.SetPosition(Window.X + 340, Window.Y + 60);
        }

        public override void Update(float dt)
        {
            Caption.Update(dt);
            Window.Update(dt);
            TextTopicSaveName.Update(dt);
            TextTopicSaveDate.Update(dt);
            List.Update(dt);
            TextNoSaves.Update(dt);
        }

        public override void Draw()
        {
            AssetVoid.DrawUnscaled(0, 0, Love.Graphics.GetWidth(), Love.Graphics.GetHeight());
            Caption.Draw();
            Window.Draw();

            if (List.Count == 0)
            {
                TextNoSaves.Draw();
            }
            else
            {
                TextTopicSaveName.Draw();
                TextTopicSaveDate.Draw();
                List.Draw();
                ScreenshotBox.Draw();
            }
        }

        public override void Dispose()
        {
            Window.Dispose();
            Caption.Dispose();
            TextTopicSaveName.Dispose();
            TextTopicSaveDate.Dispose();
            List.Dispose();
            TextNoSaves.Dispose();
            AssetNoScreenshot.Dispose();
            foreach (var image in _imageCache.Values)
            {
                image.Dispose();
            }
            _imageCache.Clear();
        }
    }
}
