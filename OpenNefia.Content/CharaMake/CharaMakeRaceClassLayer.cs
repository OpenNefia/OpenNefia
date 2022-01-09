using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.Containers;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CharaMake
{
    [Localize("Elona.CharaMake.RaceSelect")]
    public class CharaMakeRaceSelectLayer : CharaMakeRaceClassLayer
    {
        public const string ResultName = "race";
        public override IEnumerable<RaceClass> GetData()
        {
            return _prototypeManager.EnumeratePrototypes<RacePrototype>().OrderBy(x => x.IsExtra).Select(x => new RaceClass(x));
        }

        protected override void Select(RaceClass item)
        {
            Finish(new CharaMakeResult(new Dictionary<string, object>
            {
                { ResultName, item.Data }
            }));
        }
    }
    [Localize("Elona.CharaMake.ClassSelect")]
    public class CharaMakeClassSelectLayer : CharaMakeRaceClassLayer
    {
        public const string ResultName = "class";
        private UiText RaceText;
        private TileAtlasBatch Atlas = default!;
        private ChipPrototype MaleChip = default!;
        private ChipPrototype FemaleChip = default!;

        public CharaMakeClassSelectLayer()
        {
            RaceText = new UiText();
            Atlas = new TileAtlasBatch(AtlasNames.Chip);
        }

        public override void Initialize(CharaMakeData args)
        {
            base.Initialize(args);
            
            if (Data.CharaData.TryGetValue(typeof(CharaMakeRaceSelectLayer), out var vals))
            {
                if (vals.TryGetValue(CharaMakeRaceSelectLayer.ResultName, out var raceObj) && raceObj is RacePrototype race)
                {
                    RaceText.Text = $"{Loc.GetString("Elona.CharaMake.ClassSelect.RaceLabel")}: {Loc.GetPrototypeString(race.GetStrongID(), "Name")}";
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
        }

        public override IEnumerable<RaceClass> GetData()
        {
            return _prototypeManager.EnumeratePrototypes<ClassPrototype>().OrderBy(x => x.IsExtra).Select(x => new RaceClass(x));
        }

        protected override void Select(RaceClass item)
        {
            Finish(new CharaMakeResult(new Dictionary<string, object>
            {
                { ResultName, item.Data }
            }));
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            RaceText.SetPosition(Window.X + 470, Window.Y + 35);
        }

        public override void Draw()
        {
            base.Draw();
            RaceText.Draw();
            Atlas.Clear();
            Atlas.Add(FemaleChip.Image.AtlasIndex, Window.X + 375, Window.Y + 35, centered: true);
            Atlas.Add(MaleChip.Image.AtlasIndex, Window.X + 405, Window.Y + 35, centered: true);
            Atlas.Flush();
            Atlas.Draw(0, 0, Width, Height);
        }
    }

    public abstract class CharaMakeRaceClassLayer : CharaMakeLayer
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
                return (IsExtra ? $"{Loc.GetString("Elona.CharaMake.Extra")}" : string.Empty) + EnsureString("Name");
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

        [Localize] protected UiWindow Window;
        [Localize] protected UiTextTopic RaceTopic;
        [Localize] protected UiTextTopic DetailTopic;
        [Localize] protected UiTextTopic AttributeTopic;
        [Localize] protected UiTextTopic SkillTopic;
        private UiVerticalContainer DetailContainer;
        private UiVerticalContainer SkillContainer;
        private UiVerticalContainer TrainedSkillContainer;
        private UiWrapText DetailText;

        private UiGridContainer AttributeContainer;
        private UiPagedList<RaceClass> List;
        private RaceClassCell[] AllData;

        //skills that shouldn't show up on the screen at all
        private readonly string[] SpecialSkillIds = new[]
        {
            "Elona.StatLife",
            "Elona.StatMana",
            "Elona.StatLuck",
            "Elona.StatSpeed"
        };
        
        public CharaMakeRaceClassLayer()
        {
            Window = new UiWindow();
            AllData = Array.Empty<RaceClassCell>();

            RaceTopic = new UiTextTopic();
            DetailTopic = new UiTextTopic();
            AttributeTopic = new UiTextTopic();
            SkillTopic = new UiTextTopic();

            DetailContainer = new UiVerticalContainer();

            DetailText = new UiWrapText(450, UiFonts.ListTitleScreenText);
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
            List.EventOnActivate += (_, args) =>
            {
                Select(args.SelectedCell.Data);
            };
            List.EventOnSelect += (_, args) =>
            {
                SelectData(args.SelectedCell.Data);
            };

            AddChild(List);
            AddChild(Window);
        }

        public override void Initialize(CharaMakeData args)
        {
            base.Initialize(args);
            AllData = GetData().Select(x => new RaceClassCell(x)).ToArray();
            List.Clear();
            List.AddRange(AllData);

            SelectData(AllData.First().Data);
        }

        public abstract IEnumerable<RaceClass> GetData();

        public override void OnFocused()
        {
            base.OnFocused();
            List.GrabFocus();
        }

        protected abstract void Select(RaceClass item);

        private void SelectData(RaceClass data)
        {
            SetAttributes(data.GetSkills());
            SetTrainedSkills(data.GetSkills());
            DetailContainer.Resolve();

            data.TryGetString(out var desc, "Description");
            DetailText.Text = desc;
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
            foreach(var attrId in AttributeIds)
            {
                var arrtProtId = new PrototypeId<SkillPrototype>(attrId);
                skills.TryGetValue(arrtProtId, out var amt);
                GetAttributeAmountDesc(amt, out var desc, out var clr);
                yield return MakeSkillContainer(attrId, $"{Loc.GetPrototypeString(arrtProtId, "ShortName")?.Trim().ToLower().FirstCharToUpper()}: {desc}", clr);
            }
        }

        //cant actually yield here sadly because the weapon proficiencies need to be collected and be on the first position
        private IEnumerable<UiElement> MakeTrainedSkills(IReadOnlyDictionary<PrototypeId<SkillPrototype>, int> skills)
        {
            var list = new List<UiElement>();

            var profs = new List<string>();
            foreach (var skillId in skills.Keys)
            {
                if (AttributeIds.Any(x => x == skillId.ToString() || SpecialSkillIds.Any(x => x == skillId.ToString())))
                    continue;

                var skill = _prototypeManager.Index(skillId);
                switch (skill.SkillType)
                {
                    case SkillType.WeaponProficiency:
                        profs.Add(Loc.GetPrototypeString(skillId, "Name")!);
                        break;
                    default:
                        var related = _prototypeManager.Index(skill.RelatedSkill ?? default!);
                        var skillName = Loc.GetPrototypeString(skillId, "Name") ?? string.Empty;
                        var skillDesc = $"{skillName}{new string(' ', Math.Max(16 - skillName.Length, 0))}{Loc.GetPrototypeString(skillId, "Description") ?? string.Empty}";
                        var cont = MakeSkillContainer(related.ID, skillDesc);
                        list.Add(cont);
                        break;
                }
            }
            if (profs.Count > 0)
            {
                var profDesc = $"{Loc.GetString("Elona.CharaMake.ProficientIn")} {string.Join(',', profs)}";
                list.Insert(0, MakeSkillContainer("Elona.StatStrength", profDesc));
            }

            return list;
        }

        private void GetAttributeAmountDesc(int amount, out string text, out Color color)
        {
            switch (amount)
            {
                case <= 0:
                    text = Loc.GetString("Elona.CharaMake.Skill.Amount.None");
                    color = UiColors.CharaMakeStatLevelNone;
                    break;
                case <= 2:
                    text = Loc.GetString("Elona.CharaMake.Skill.Amount.Slight");
                    color = UiColors.CharaMakeStatLevelSlight;
                    break;
                case <= 4:
                    text = Loc.GetString("Elona.CharaMake.Skill.Amount.Little");
                    color = UiColors.CharaMakeStatLevelLittle;
                    break;
                case <= 6:
                    text = Loc.GetString("Elona.CharaMake.Skill.Amount.Normal");
                    color = UiColors.CharaMakeStatLevelNormal;
                    break;
                case <= 8:
                    text = Loc.GetString("Elona.CharaMake.Skill.Amount.NotBad");
                    color = UiColors.CharaMakeStatLevelNotBad;
                    break;
                case <= 10:
                    text = Loc.GetString("Elona.CharaMake.Skill.Amount.Good");
                    color = UiColors.CharaMakeStatLevelGood;
                    break;
                case <= 12:
                    text = Loc.GetString("Elona.CharaMake.Skill.Amount.Great");
                    color = UiColors.CharaMakeStatLevelGreat;
                    break;
                default:
                    text = Loc.GetString("Elona.CharaMake.Skill.Amount.Best");
                    color = UiColors.CharaMakeStatLevelBest;
                    break;
            }
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Window.SetSize(700, 500);
            List.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Center(Window);
            List.SetPosition(Window.X + 35, Window.Y + 60);
            RaceTopic.SetPosition(Window.X + 30, Window.Y + 30);
            DetailTopic.SetPosition(Window.X + 190, RaceTopic.Y);
            DetailContainer.SetPosition(Window.X + 210, Window.Y + 60);
            DetailContainer.Resolve();
        }

        public override void Draw()
        {
            base.Draw();
            Window.Draw();
            GraphicsEx.SetColor(255, 255, 255, 50);
            CurrentWindowBG.Draw(Window.X + 15, Window.Y + 40, 270, 420);
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
