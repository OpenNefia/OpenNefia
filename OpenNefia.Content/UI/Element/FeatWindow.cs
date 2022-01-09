using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Element
{
    public class FeatWindow : UiWindow
    {
        public abstract record FeatData
        {
            public virtual string Name { get; } = "";
            public virtual string Description { get; } = "";
            public record Feat(FeatPrototype Prototype, int Level) : FeatData
            {
                public override string Name => Loc.GetPrototypeString(Prototype.GetStrongID(), $"{Level + 1}.Name")!;
            }
            public record FeatHeader(string LocalizeText) : FeatData
            {
                public override string Name => Loc.GetString(LocalizeText);
            }
            public record GainedFeat : Feat
            {
                public bool IsRace;
                
                public GainedFeat(FeatPrototype Prototype, int Level, bool isRace) : base(Prototype, Level)
                {
                    IsRace = isRace;
                }
            }
        }

        public class FeatCell : UiListCell<FeatData>
        {
            public static explicit operator FeatCell(FeatData data) => new FeatCell(data);

            public FeatCell(FeatData data) 
                : base(data, new UiText())
            {
                Text = data.Name;
            }

            public override void SetPosition(int x, int y)
            {
                switch (Data)
                {
                    default:
                        base.SetPosition(x, y);
                        break;
                    case FeatData.FeatHeader header:
                        base.SetPosition(x + 30, y);
                        break;
                    case FeatData.GainedFeat gained:
                        base.SetPosition(x + 15, y);
                        break;
                }
            }

            public override void Draw()
            {
                if (IndexInList % 2 == 0 && Data is not FeatData.FeatHeader)
                {
                    Love.Graphics.SetColor(UiColors.ListEntryAccent);
                    Love.Graphics.Rectangle(Love.DrawMode.Fill, X - 1, Y, 650, 18);
                }
                switch (Data)
                {
                    case FeatData.GainedFeat:
                    case FeatData.FeatHeader:
                        UiText.Draw();
                        break;
                    case FeatData.Feat:
                        base.Draw();
                        break;
                }
            }
        }

        protected readonly IPrototypeManager _prototypeManager = default!;

        private const int WindowHeight = 430;
        private const int WindowWidth = 740;
        private UiPageModel<FeatCell> PageModel;
        public UiList<FeatData> List;

        private Func<Dictionary<FeatPrototype, int>> GetFeatsFunc;
        private Action<FeatData.Feat> SelectFeatAction;

        public FeatWindow(Func<Dictionary<FeatPrototype, int>> getFeatsFunc, Action<FeatData.Feat> selectFeatAction)
        {
            _prototypeManager = IoCManager.Resolve<IPrototypeManager>();

            TextTitle.Text = Loc.GetString("Elona.FeatMenu.Title");
            List = new UiList<FeatData>();
            GetFeatsFunc = getFeatsFunc ?? (() => new Dictionary<FeatPrototype, int>());
            SelectFeatAction = selectFeatAction ?? (feat => { });
            PageModel = new UiPageModel<FeatCell>();
            PageModel.SetWindow(this);
            PageModel.NumberXOffset = -55;
            PageModel.NumberYOffset = -3;

            List.GrabFocus();
            List.EventOnActivate += List_OnActivate;
            PageModel.OnPageChanged += OnPageChanged;
            RefreshData();
        }

        private void RefreshData(FeatPrototype? lastSelected = null)
        {
            var data = new List<FeatCell>();
            var prototypes = _prototypeManager.EnumeratePrototypes<FeatPrototype>()?.Where(x => x.FeatType == FeatType.Feat)!;

            var gainedFeats = GetFeatsFunc();

            data.Add(new FeatCell(new FeatData.FeatHeader("Elona.FeatMenu.AvailableHeader")));
            data.AddRange(prototypes.Select(x =>
            {
                gainedFeats.TryGetValue(x, out var val);
                return new FeatCell(new FeatData.Feat(x, val));
            }));
            data.Add(new FeatCell(new FeatData.FeatHeader("Elona.FeatMenu.GainedHeader")));
            data.AddRange(gainedFeats
                .Select(x => new FeatCell(new FeatData.GainedFeat(x.Key, x.Value, x.Key.FeatType == FeatType.Race)))
                .OrderBy(x => (x.Data as FeatData.GainedFeat)?.IsRace));
            PageModel.Initialize(data, 15);

            if (lastSelected != null)
            {
                PageModel.ChangePage(PageModel.PageCount);
                List.Select(List.IndexOf(List.First(x => (x.Data as FeatData.Feat)?.Prototype == lastSelected)));
            }
        }

        private void List_OnActivate(object? sender, UiListEventArgs<FeatData> args)
        {
            switch(args.SelectedCell.Data)
            {
                case FeatData.Feat feat:
                    SelectFeatAction(feat);
                    RefreshData(feat.Prototype);
                    break;
            }
        }

        private void OnPageChanged()
        {
            List.Clear();
            List.AddRange(PageModel.GetCurrentElements());
            List.Select(List.SelectedIndex);
        }

        public void OnKeyDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UINextPage)
            {
                PageModel.PageForward();
            }
            if (args.Function == EngineKeyFunctions.UIPreviousPage)
            {
                PageModel.PageBackward();
            }
        }

        public override void GetPreferredSize(out Vector2i size)
        {
            size.X = WindowWidth;
            size.Y = WindowHeight;
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(WindowWidth, WindowHeight);
            List.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            List.SetPosition(X + 55, Y + 60);
            PageModel.SetPosition(x, y);
        }

        public override void Draw()
        {
            base.Draw();
            List.Draw();
            PageModel.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            List.Update(dt);
            PageModel.Update(dt);
        }

        public override void Dispose()
        {
            PageModel.OnPageChanged -= OnPageChanged;
            List.EventOnActivate -= List_OnActivate;
        }
    }
}
