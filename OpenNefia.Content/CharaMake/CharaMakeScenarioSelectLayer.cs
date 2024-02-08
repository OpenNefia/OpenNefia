using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Scenarios;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.Containers;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Audio;

namespace OpenNefia.Content.CharaMake
{
    [Localize("Elona.CharaMake.ScenarioSelect")]
    public sealed class CharaMakeScenarioSelectLayer : CharaMakeLayer<CharaMakeScenarioSelectLayer.ResultData>
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public sealed record ScenarioData(PrototypeId<ScenarioPrototype> ScenarioID);

        public sealed class ScenarioCell : UiListCell<ScenarioData>
        {
            public ScenarioCell(ScenarioData data)
                : base(data, new UiText(UiFonts.ListText))
            {
                if (Loc.TryGetPrototypeString(Data.ScenarioID, "Name", out var name))
                    Text = name;
                else
                    Text = $"<{Data.ScenarioID}>";
            }
        }

        [Child] private UiVerticalContainer DetailContainer = new();
        [Child][Localize] protected UiWindow Window = new();
        [Child][Localize] protected UiTextTopic NameTopic = new();
        [Child][Localize] protected UiTextTopic DetailTopic = new();
        [Child] private UiPagedList<ScenarioData> List;

        public sealed class ResultData : CharaMakeResult
        {
            public PrototypeId<ScenarioPrototype> ScenarioID { get; set; } = Protos.Scenario.Default;

            public ResultData(PrototypeId<ScenarioPrototype> scenarioID)
            {
                ScenarioID = scenarioID;
            }

            public override void ApplyStep(EntityUid entity, EntityGenArgSet args)
            {
            }
        }

        //
        // DetailContainer children
        // 
        private UiWrappedText DetailText = new(UiFonts.ListTitleScreenText);

        public CharaMakeScenarioSelectLayer()
        {
            DetailText.MinSize = new(450, 0);
            DetailContainer.AddElement(DetailText);
            DetailContainer.AddLayout(LayoutType.YMin, 110);

            List = new UiPagedList<ScenarioData>(itemsPerPage: 16, elementForPageText: Window);
            List.OnActivated += (_, args) =>
            {
                HandleActivate(args.SelectedCell.Data);
            };
            List.OnSelected += (_, args) =>
            {
                HandleSelect(args.SelectedCell.Data);
            };
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            List.GrabFocus();
        }

        private void HandleActivate(ScenarioData item)
        {
            Sounds.Play(Protos.Sound.Ok1);
            Finish(new CharaMakeUIResult(new ResultData(item.ScenarioID)));
        }

        private void HandleSelect(ScenarioData data)
        {
            DetailText.OriginalText = string.Empty;
            if (Loc.TryGetPrototypeString(data.ScenarioID, "Description", out var desc))
                DetailText.OriginalText = desc;
            DetailContainer.Relayout();
        }

        public override void Initialize(CharaMakeResultSet args)
        {
            base.Initialize(args);
            var data = _protos.EnumeratePrototypes<ScenarioPrototype>().Select(x => new ScenarioData(x.GetStrongID())).ToArray();
            Window.KeyHints = MakeKeyHints();
            List.SetCells(data.Select(d => new ScenarioCell(d)));

            if (data.Length > 0)
                HandleSelect(data[0]);
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(680, 500, out bounds, yOffset: 20);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            Window.SetSize(Width, Height);
            List.SetPreferredSize();
            DetailText.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            Window.SetPosition(X, Y);
            List.SetPosition(Window.X + 35, Window.Y + 60);

            NameTopic.SetPosition(Window.X + 30, Window.Y + 30);
            DetailTopic.SetPosition(Window.X + 190, NameTopic.Y);
            DetailContainer.SetPosition(Window.X + 210, Window.Y + 60);
            DetailContainer.Relayout();
        }

        public override void Draw()
        {
            base.Draw();
            Window.Draw();
            GraphicsEx.SetColor(255, 255, 255, 50);
            CurrentWindowBG.Draw(UIScale, Window.X + 15, Window.Y + 40, 270, 420);
            List.Draw();

            NameTopic.Draw();
            DetailTopic.Draw();
            DetailContainer.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Window.Update(dt);
            List.Update(dt);

            NameTopic.Update(dt);
            DetailTopic.Update(dt);
            DetailText.Update(dt);
            DetailContainer.Update(dt);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
