using OpenNefia.Content.Charas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.Containers;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.CharaMake
{
    [Localize("Elona.CharaMake.RaceSelect")]
    public class CharaMakeRaceSelectLayer : CharaMakeRaceClassLayer<CharaMakeRaceSelectLayer.ResultData>
    {
        public override IEnumerable<RaceClass> GetData()
        {
            return _prototypeManager.EnumeratePrototypes<RacePrototype>().OrderBy(x => x.IsExtra).Select(x => new RaceClass(x));
        }

        public sealed class ResultData : CharaMakeResult
        {
            public PrototypeId<RacePrototype> RaceID { get; set; }

            public ResultData(PrototypeId<RacePrototype> race)
            {
                RaceID = race;
            }

            public override void ApplyStep(EntityUid entity, EntityGenArgSet args)
            {
                if (!EntityManager.TryGetComponent<CharaComponent>(entity, out var chara))
                {
                    Logger.WarningS("charamake", "No CharaComponent present on entity");
                    return;
                }

                chara.Race = RaceID;
            }
        }

        protected override void Select(RaceClass item)
        {
            Finish(new CharaMakeUIResult(new ResultData(((RacePrototype)item.Data).GetStrongID())));
        }
    }

    [Localize("Elona.CharaMake.ClassSelect")]
    public class CharaMakeClassSelectLayer : CharaMakeRaceClassLayer<CharaMakeClassSelectLayer.ResultData>
    {
        public const string ResultName = "class";

        [Dependency] private readonly IPrototypeManager _protos = default!;

        [Child] private UiText RaceText;

        private TileAtlasBatch Atlas = default!;
        private ChipPrototype MaleChip = default!;
        private ChipPrototype FemaleChip = default!;

        public CharaMakeClassSelectLayer()
        {
            RaceText = new UiText();
            Atlas = new TileAtlasBatch(AtlasNames.Chip);
        }

        public override void Initialize(CharaMakeResultSet args)
        {
            base.Initialize(args);

            if (Results.TryGet<CharaMakeRaceSelectLayer.ResultData>(out var raceResult))
            {
                var race = _protos.Index(raceResult.RaceID);
                RaceText.Text = $"{Loc.GetString("Elona.CharaMake.ClassSelect.RaceLabel")}: {Loc.GetPrototypeString(raceResult.RaceID, "Name")}";
                try
                {
                    MaleChip = race.ChipMale.ResolvePrototype();
                }
                catch (Exception ex)
                {
                    Logger.ErrorS("charamake", ex, $"error resolving prototype for male race chip for race {race.ID}");
                }
                try
                {
                    FemaleChip = race.ChipFemale.ResolvePrototype();
                }
                catch (Exception ex)
                {
                    Logger.ErrorS("charamake", ex, $"error resolving prototype for female race chip for race {race.ID}");
                }
            }
        }

        public override IEnumerable<RaceClass> GetData()
        {
            return _prototypeManager.EnumeratePrototypes<ClassPrototype>().OrderBy(x => x.IsExtra).Select(x => new RaceClass(x));
        }

        protected override void Select(RaceClass item)
        {
            Finish(new CharaMakeUIResult(new ResultData(((ClassPrototype)item.Data).GetStrongID())));
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            RaceText.SetPosition(Window.X + 470, Window.Y + 35);
        }

        public override void Draw()
        {
            base.Draw();
            RaceText.Draw();
            Atlas.Clear();
            Atlas.Add(UIScale, FemaleChip.Image.AtlasIndex, Window.X + 375, Window.Y + 35, centering: BatchCentering.Centered);
            Atlas.Add(UIScale, MaleChip.Image.AtlasIndex, Window.X + 405, Window.Y + 35, centering: BatchCentering.Centered);
            Atlas.Flush();
            Atlas.Draw(UIScale, 0, 0);
        }

        public sealed class ResultData : CharaMakeResult
        {
            public PrototypeId<ClassPrototype> ClassID { get; set; }

            public ResultData(PrototypeId<ClassPrototype> @class)
            {
                ClassID = @class;
            }

            public override void ApplyStep(EntityUid entity, EntityGenArgSet args)
            {
                if (!EntityManager.TryGetComponent<CharaComponent>(entity, out var chara))
                {
                    Logger.WarningS("charamake", $"No {nameof(CharaComponent)} present on entity");
                    return;
                }

                chara.Class = ClassID;
            }
        }
    }

    public abstract class CharaMakeRaceClassLayer<T> : CharaMakeLayer<T>
        where T : ICharaMakeResult
    {
        public class RaceClass
        {
            public IPrototype Data;
            private bool IsExtra;

            public RaceClass(IPrototype data)
            {
                Data = data;
                IsExtra = Data switch
                {
                    RacePrototype race => race.IsExtra,
                    ClassPrototype @class => @class.IsExtra,
                    _ => false
                };
            }
            public bool TryGetString(out string result, string suffix = "")
            {
                result = EnsureString(suffix);
                if (result.StartsWith('<'))
                {
                    result = string.Empty;
                    return false;
                }
                else
                {
                    return true;
                }
            }

            public string EnsureString(string suffix = "")
            {
                switch (Data)
                {
                    case RacePrototype race:
                        return Loc.GetPrototypeString(race.GetStrongID(), suffix)!;
                    case ClassPrototype @class:
                        return Loc.GetPrototypeString(@class.GetStrongID(), suffix)!;
                    default:
                        return string.Empty;
                }
            }

            public string GetName()
            {
                return (IsExtra ? $"{Loc.GetString("Elona.CharaMake.ClassAndRaceSelect.Extra")}" : string.Empty) + EnsureString("Name");
            }

            public IReadOnlyDictionary<PrototypeId<SkillPrototype>, int> GetSkills()
            {
                switch (Data)
                {
                    case RacePrototype race:
                        return race.BaseSkills;
                    case ClassPrototype @class:
                        return @class.BaseSkills;
                    default:
                        return new Dictionary<PrototypeId<SkillPrototype>, int>();
                }
            }
        }

        public class RaceClassCell : UiListCell<RaceClass>
        {
            public RaceClassCell(RaceClass wrapper)
                : base(wrapper, new UiText(UiFonts.ListText))
            {
                Text = Data.GetName();
            }
        }

        [Dependency] protected readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] protected readonly ISkillsSystem _skillsSys = default!;

        [Child][Localize] protected UiWindow Window;
        [Child][Localize] protected UiTextTopic RaceTopic;
        [Child][Localize] protected UiTextTopic DetailTopic;
        [Child] private UiVerticalContainer DetailContainer;
        [Child] private UiPagedList<RaceClass> List;

        //
        // DetailContainer children
        // 
        [Localize] protected UiTextTopic AttributeTopic;
        [Localize] protected UiTextTopic SkillTopic;
        private UiVerticalContainer SkillContainer;
        private UiVerticalContainer TrainedSkillContainer;
        private UiWrappedText DetailText;

        private UiGridContainer AttributeContainer;
        private RaceClassCell[] AllData;

        public CharaMakeRaceClassLayer()
        {
            Window = new UiWindow();
            AllData = Array.Empty<RaceClassCell>();

            RaceTopic = new UiTextTopic();
            DetailTopic = new UiTextTopic();
            AttributeTopic = new UiTextTopic();
            SkillTopic = new UiTextTopic();

            DetailContainer = new UiVerticalContainer();

            DetailText = new UiWrappedText(UiFonts.ListTitleScreenText);
            DetailText.MinSize = new(450, 0);
            DetailContainer.AddElement(DetailText);
            DetailContainer.AddLayout(LayoutType.YMin, 110);

            DetailContainer.AddLayout(LayoutType.XOffset, -3);
            DetailContainer.AddElement(AttributeTopic, LayoutType.XOffset, -7);

            DetailContainer.AddLayout(LayoutType.Spacer, 18);

            AttributeContainer = new UiGridContainer(GridType.Horizontal, 3, xCentered: false, xSpace: 50, ySpace: 3);
            DetailContainer.AddElement(AttributeContainer);

            DetailContainer.AddLayout(LayoutType.Spacer, 5);

            DetailContainer.AddElement(SkillTopic, LayoutType.XOffset, -7);
            SkillContainer = new UiVerticalContainer();
            DetailContainer.AddElement(SkillContainer);

            TrainedSkillContainer = new UiVerticalContainer();
            DetailContainer.AddLayout(LayoutType.Spacer, 18);
            DetailContainer.AddElement(TrainedSkillContainer);

            List = new UiPagedList<RaceClass>(itemsPerPage: 16, elementForPageText: Window);
            List.OnActivated += (_, args) =>
            {
                Select(args.SelectedCell.Data);
            };
            List.OnSelected += (_, args) =>
            {
                SelectData(args.SelectedCell.Data);
            };
        }

        public override void Initialize(CharaMakeResultSet args)
        {
            base.Initialize(args);
            AllData = GetData().Select(x => new RaceClassCell(x)).ToArray();
            Window.KeyHints = MakeKeyHints();
            List.SetCells(AllData);

            SelectData(AllData.First().Data);
        }

        public abstract IEnumerable<RaceClass> GetData();

        public override void GrabFocus()
        {
            base.GrabFocus();
            List.GrabFocus();
        }

        protected abstract void Select(RaceClass item);

        private void SelectData(RaceClass data)
        {
            SetAttributes(data.GetSkills());
            SetTrainedSkills(data.GetSkills());
            DetailContainer.Relayout();

            data.TryGetString(out var desc, "Description");
            DetailText.WrappedText = desc;
        }

        private void SetAttributes(IReadOnlyDictionary<PrototypeId<SkillPrototype>, int> skills)
        {
            AttributeContainer.Clear();
            AttributeContainer.AddLayout(LayoutType.XMin, 100);

            foreach (var attr in MakeDetailAttribute(skills))
            {
                AttributeContainer.AddElement(attr);
            }
        }

        private void SetTrainedSkills(IReadOnlyDictionary<PrototypeId<SkillPrototype>, int> skills)
        {
            TrainedSkillContainer.Clear();
            foreach (var attr in MakeTrainedSkills(skills))
            {
                TrainedSkillContainer.AddElement(attr);
            }
        }

        private IEnumerable<UiElement> MakeDetailAttribute(IReadOnlyDictionary<PrototypeId<SkillPrototype>, int> skills)
        {
            foreach (var attrProto in _skillsSys.EnumerateBaseAttributes())
            {
                var attrProtoId = attrProto.GetStrongID();
                skills.TryGetValue(attrProtoId, out var amt);
                GetAttributeAmountDesc(amt, out var desc, out var clr);
                yield return MakeSkillContainer(attrProtoId, $"{Loc.GetPrototypeString(attrProtoId, "ShortName")?.Trim().ToLower().FirstCharToUpper()}: {desc}", clr);
            }
        }

        //cant actually yield here sadly because the weapon proficiencies need to be collected and be on the first position
        private IEnumerable<UiElement> MakeTrainedSkills(IReadOnlyDictionary<PrototypeId<SkillPrototype>, int> skills)
        {
            var list = new List<UiElement>();

            var profs = new List<string>();
            foreach (var skillId in skills.Keys)
            {
                var skill = _prototypeManager.Index(skillId);

                if (skill.SkillType == SkillType.Attribute)
                    continue;

                switch (skill.SkillType)
                {
                    case SkillType.WeaponProficiency:
                        profs.Add(Loc.GetPrototypeString(skillId, "Name")!);
                        break;
                    default:
                        var related = _prototypeManager.Index(skill.RelatedSkill ?? default!);
                        var skillName = Loc.GetPrototypeString(skillId, "Name") ?? string.Empty;
                        var skillDesc = $"{skillName.WidePadRight(16)}{Loc.GetPrototypeString(skillId, "Description") ?? string.Empty}";
                        var cont = MakeSkillContainer(related.GetStrongID(), skillDesc);
                        list.Add(cont);
                        break;
                }
            }
            if (profs.Count > 0)
            {
                var profDesc = $"{Loc.GetString("Elona.CharaMake.ClassAndRaceSelect.ProficientIn")} {string.Join(',', profs)}";
                list.Insert(0, MakeSkillContainer(Protos.Skill.AttrStrength, profDesc));
            }

            return list;
        }

        private void GetAttributeAmountDesc(int amount, out string text, out Core.Maths.Color color)
        {
            switch (amount)
            {
                case <= 0:
                    text = Loc.GetString("Elona.CharaMake.Skill.Amount.None");
                    color = UiColors.CharaMakeAttrLevelNone;
                    break;
                case <= 2:
                    text = Loc.GetString("Elona.CharaMake.Skill.Amount.Slight");
                    color = UiColors.CharaMakeAttrLevelSlight;
                    break;
                case <= 4:
                    text = Loc.GetString("Elona.CharaMake.Skill.Amount.Little");
                    color = UiColors.CharaMakeAttrLevelLittle;
                    break;
                case <= 6:
                    text = Loc.GetString("Elona.CharaMake.Skill.Amount.Normal");
                    color = UiColors.CharaMakeAttrLevelNormal;
                    break;
                case <= 8:
                    text = Loc.GetString("Elona.CharaMake.Skill.Amount.NotBad");
                    color = UiColors.CharaMakeAttrLevelNotBad;
                    break;
                case <= 10:
                    text = Loc.GetString("Elona.CharaMake.Skill.Amount.Good");
                    color = UiColors.CharaMakeAttrLevelGood;
                    break;
                case <= 12:
                    text = Loc.GetString("Elona.CharaMake.Skill.Amount.Great");
                    color = UiColors.CharaMakeAttrLevelGreat;
                    break;
                default:
                    text = Loc.GetString("Elona.CharaMake.Skill.Amount.Best");
                    color = UiColors.CharaMakeAttrLevelBest;
                    break;
            }
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
            RaceTopic.SetPosition(Window.X + 30, Window.Y + 30);
            DetailTopic.SetPosition(Window.X + 190, RaceTopic.Y);
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

            RaceTopic.Draw();
            DetailTopic.Draw();
            DetailContainer.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Window.Update(dt);
            List.Update(dt);

            RaceTopic.Update(dt);
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
