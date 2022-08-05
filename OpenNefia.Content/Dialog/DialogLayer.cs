using OpenNefia.Content.Charas;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Fame;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Portraits;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Religion;
using OpenNefia.Content.Rendering;
using OpenNefia.Content.Shopkeeper;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Content.Dialog
{
    public sealed class DialogChoiceData
    {
        public DialogChoiceData(DialogChoice choice)
        {
            Choice = choice;
        }

        public DialogChoice Choice { get; }
    }

    public class DialogChoiceCell : UiListCell<DialogChoiceData>
    {
        public DialogChoiceCell(DialogChoiceData data)
            : base(data, new UiText(data.Choice.Text))
        {
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
        }

        public override void Draw()
        {
            base.Draw();
        }
    }

    public class DialogArgs
    {
    }

    public class DialogResult
    {
        public DialogResult(int index)
        {
            SelectedChoiceIndex = index;
        }

        public int SelectedChoiceIndex { get; }
    }

    public class DialogStepData
    {
        public EntityUid? Target { get; set; }
        public string SpeakerName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public List<DialogChoice> Choices { get; set; } = new();
        public bool CanCancel { get; set; } = false;
    }

    public sealed class DialogChoice
    {
        public string Text { get; set; } = string.Empty;
    }

    public interface IDialogLayer : IUiLayerWithResult<DialogArgs, DialogResult>
    {
        void UpdateFromStepData(DialogStepData data);
    }

    [Localize("Elona.Dialog.Layer")]
    public sealed class DialogLayer : UiLayerWithResult<DialogArgs, DialogResult>, IDialogLayer
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IDialogSystem _dialog = default!;
        [Dependency] private readonly IFieldLayer _field = default!;

        private IAssetInstance _assetIeChat = default!;
        private IAssetInstance _assetImpressionIcon = default!;

        private TileAtlasBatch _chipBatch = new TileAtlasBatch(AtlasNames.Chip);
        private TileAtlasBatch _portraitBatch = new TileAtlasBatch(ContentAtlasNames.Portrait);

        [Child][Localize("Topic.Impress")] private UiText TopicImpress = new UiTextTopic();
        [Child][Localize("Topic.Attract")] private UiText TopicAttract = new UiTextTopic();
        [Child] private UiText TextSpeakerName = new(UiFonts.DialogSpeakerName);
        [Child] private UiText TextImpression = new(UiFonts.DialogImpressionText);
        [Child] private UiText TextImpression2 = new(UiFonts.DialogImpressionText);
        [Child] private UiWrappedText TextBody = new(UiFonts.DialogBodyText);
        [Child] private UiList<DialogChoiceData> List = new();

        private EntityUid? _target = null;
        private PortraitPrototype? _portrait = null;
        private ChipPrototype? _chip = null;
        private Color _chipColor = Color.Black;
        private int _interestIconCount = 0;
        private bool _canCancel = false;

        public override void Initialize(DialogArgs args)
        {
            Sounds.Play(Protos.Sound.Chat);

            _assetIeChat = Assets.Get(Protos.Asset.IeChat);
            _assetImpressionIcon = Assets.Get(Protos.Asset.ImpressionIcon);
            
            CanControlFocus = false;
            EventFilter = UIEventFilterMode.Stop;
            OnKeyBindDown += HandleKeyBindDown;
            List.OnActivated += List_OnActivated;
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            List.GrabFocus();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel && _canCancel)
            {
                Sounds.Play(Protos.Sound.More1);
                Cancel();
                args.Handle();
            }
        }

        private void List_OnActivated(object? sender, UiListEventArgs<DialogChoiceData> e)
        {
            Sounds.Play(Protos.Sound.More1);
            Finish(new(e.SelectedIndex));
        }

        public void UpdateFromStepData(DialogStepData data)
        {
            List.SetCells(data.Choices.Select(c => new DialogChoiceCell(new(c))).ToList());
            List.Select(0);

            _portrait = null;
            _chip = null;
            _chipColor = Color.Black;

            _interestIconCount = 0;
            _target = data.Target;
            _canCancel = data.CanCancel;

            TextBody.WrappedText = data.Text;
            TextSpeakerName.Text = data.SpeakerName;

            if (_entityManager.IsAlive(_target))
            {
                if (_entityManager.TryGetComponent<PortraitComponent>(_target.Value, out var portrait))
                {
                    _portrait = _protos.Index(portrait.PortraitID);
                }
                else if (_entityManager.TryGetComponent<ChipComponent>(_target.Value, out var chip))
                {
                    _chip = _protos.Index(chip.ChipID);
                    _chipColor = chip.Color;
                }

                if (_entityManager.TryGetComponent<DialogComponent>(_target.Value, out var dialog))
                {
                    var impressionLevel = _dialog.GetImpressionLevel(dialog.Impression);
                    var impressionText = Loc.GetString($"Elona.Dialog.Impression.Levels.{impressionLevel}");
                    string impressionValue;
                    if (dialog.Impression < ImpressionLevels.Fellow)
                        impressionValue = dialog.Impression.ToString();
                    else
                        impressionValue = "???";

                    TextImpression.SetPosition(X + 32, Y + 198);
                    TextImpression.Text = $"({impressionValue}){impressionText}";

                    if (dialog.Interest >= 0)
                    {
                        _interestIconCount = Math.Min(dialog.Interest / 5 + 1, 20);
                    }
                }
                else
                {
                    TextImpression.SetPosition(X + 60, Y + 198);
                    TextImpression.Text = "-";
                    TextImpression2.Text = "-";
                }
            }
            else
            {
                TextSpeakerName.Text = "";
                TextImpression.Text = "-";
                TextImpression2.Text = "-";
            }

            _field.RefreshScreen();
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(new(600, 380), out bounds);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            TopicImpress.SetPreferredSize();
            TopicAttract.SetPreferredSize();
            TextSpeakerName.SetPreferredSize();
            TextImpression.SetPreferredSize();
            TextImpression2.SetPreferredSize();
            TextBody.SetSize(Width - 200, Height);
            List.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            TopicImpress.SetPosition(X + 28, Y + 170);
            TopicAttract.SetPosition(X + 28, Y + 215);
            TextSpeakerName.SetPosition(X + 120, Y + 16);
            if (_entityManager.IsAlive(_target) && _entityManager.HasComponent<DialogComponent>(_target.Value))
                TextImpression.SetPosition(X + 32, Y + 198);
            else
                TextImpression.SetPosition(X + 60, Y + 198);
            TextImpression2.SetPosition(X + 60, Y + 245);
            TextBody.SetPosition(X + 150, Y + 43);
            List.SetPosition(X + 136, Y + Height - 56 - List.Height + 4);
        }

        public override void Update(float dt)
        {
            TopicImpress.Update(dt);
            TopicAttract.Update(dt);
            TextSpeakerName.Update(dt);
            TextImpression.Update(dt);
            TextImpression2.Update(dt);
            TextBody.Update(dt);
            List.Update(dt);
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Color.White.WithAlphaB(80));
            _assetIeChat.Draw(UIScale, X + 4, Y - 16);
            Love.Graphics.SetColor(Color.White);
            _assetIeChat.Draw(UIScale, X, Y - 20);

            if (_portrait != null)
            {
                _portraitBatch.Clear();
                _portraitBatch.Add(UIScale, _portrait.Image.AtlasIndex, 0, 0, 80, 112);
                _portraitBatch.Draw(UIScale, X + 42, Y + 42);
            }
            else if (_chip != null)
            {
                var (chipWidth, chipHeight) = _chipBatch.GetTileSize(_chip.Image);
                _chipBatch.Clear();
                _chipBatch.Add(UIScale, _chip.Image.AtlasIndex, 0, 0);
                _chipBatch.Draw(UIScale, X + 82 + chipWidth, Y + 125 + chipHeight, chipWidth * 2, chipHeight * 2, _chipColor);
            }

            TopicImpress.Draw();
            TopicAttract.Draw();
            TextSpeakerName.Draw();

            TextImpression.Draw();
            TextImpression2.Draw();

            Love.Graphics.SetColor(Color.White);
            for (var i = 0; i < _interestIconCount; i++)
            {
                _assetImpressionIcon.Draw(UIScale, X + 26 + i * 4, Y + 245);
            }

            TextBody.Draw();
            List.Draw();
        }

        public override void Dispose()
        {
            TopicImpress.Dispose();
            TopicAttract.Dispose();
            TextSpeakerName.Dispose();
            TextImpression.Dispose();
            TextImpression2.Dispose();
            TextBody.Dispose();
            List.Dispose();
        }
    }
}
