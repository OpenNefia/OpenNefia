using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Maths;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Charas;
using OpenNefia.Content.Skills;
using OpenNefia.Content.World;
using OpenNefia.Content.Levels;
using OpenNefia.Content.UI.Element.Containers;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Cargo;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Core.UI;
using OpenNefia.Content.Equipment;

namespace OpenNefia.Content.UI.Element
{
    public class CharaSheet : UiElement
    {
        public static class CharaSheetHelpers
        {
            public static string GetPotentialDescription(int pot)
            {
                if (pot >= 200)
                    return Loc.GetString("Elona.CharaSheet.Potential.Superb");
                else if (pot >= 150)
                    return Loc.GetString("Elona.CharaSheet.Potential.Great");
                else if (pot >= 100)
                    return Loc.GetString("Elona.CharaSheet.Potential.Good");
                else if (pot >= 50)
                    return Loc.GetString("Elona.CharaSheet.Potential.Bad");
                else
                    return Loc.GetString("Elona.CharaSheet.Potential.Hopeless");
            }
        }

        public class HexAndBlessingIcon : UiElement
        {
            private const int TileWidth = 32;
            private const int TileHeight = 32;

            private IAssetInstance HexBlessingIcons;
            private IAssetInstance TileIcon;

            private PrototypeId<BuffPrototype>? Buff;

            public HexAndBlessingIcon(PrototypeId<BuffPrototype>? buff)
            {
                TileIcon = Assets.Get(Protos.Asset.BuffIconNone);
                HexBlessingIcons = Assets.Get(Protos.Asset.BuffIcons);
                Buff = buff;
            }

            public override void GetPreferredSize(out Vector2i size)
            {
                size.X = TileWidth;
                size.Y = TileHeight;
            }

            public override void Draw()
            {
                base.Draw();
                GraphicsEx.SetColor(255, 255, 255, 120);
                var x = X + (TileWidth / 2);
                var y = Y + (TileHeight / 2);
                TileIcon.Draw(x, y, centered:true);
                if (Buff.HasValue)
                {
                    GraphicsEx.SetColor(Color.White);
                    var buffProto = Buff?.ResolvePrototype()!;
                    HexBlessingIcons.DrawRegion(buffProto.RegionId, x, y);
                }
            }

        }    

        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] protected readonly ISkillsSystem _skillsSys = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly ICargoSystem _cargoSys = default!;

        public const int SheetWidth = 700;
        public const int SheetHeight = 400;
        private const int ContainerSpacing = 4;
        private const int AttributeContainerSpacing = 6;
        private const int AttributeSpacing = 5;
        private const int AttributePotentialSpacing = 8;

        private IAssetInstance IeSheet;
        private EntityUid CharaEntity;
        private UiTopicWindow FaceFrame;
        private UiContainer NameContainer;
        private UiContainer ClassContainer;
        private UiContainer ExpContainer;
        private UiContainer AttributeContainer;
        private UiContainer SpecialStatContainer;
        private UiContainer BlessingContainer;
        private UiContainer TraceContainer;
        private UiContainer ExtraContainer;
        private UiContainer RollsContainer;

        // Temporary variables that need to be replaced as soon as the containing components exist
        private string TempName = "????";
        private string TempGod = "Eyth of Infidel";
        private string TempGuild = "None";
        private string TempSanity = "0";
        private string TempFame = "0";
        private string TempKarma = "0";
        private string TempKills = "0";

        public CharaSheet(EntityUid charaEntity)
        {
            EntitySystem.InjectDependencies(this);
            CharaEntity = charaEntity;
            IeSheet = Assets.Get(Protos.Asset.IeSheet);
            FaceFrame = new UiTopicWindow();

            NameContainer = new UiVerticalContainer { YSpace = ContainerSpacing };
            ClassContainer = new UiVerticalContainer { YSpace = ContainerSpacing };
            ExpContainer = new UiVerticalContainer { YSpace = ContainerSpacing };
            AttributeContainer = new UiVerticalContainer { YSpace = AttributeContainerSpacing };
            SpecialStatContainer = new UiVerticalContainer { YSpace = ContainerSpacing };
            BlessingContainer = new UiVerticalContainer();
            TraceContainer = new UiVerticalContainer { YSpace = ContainerSpacing };
            ExtraContainer = new UiVerticalContainer { YSpace = ContainerSpacing };
            RollsContainer = new UiVerticalContainer { YSpace = ContainerSpacing };
            Init();
        }

        public void Init()
        {
            NameContainer.Clear();
            ClassContainer.Clear();
            ExpContainer.Clear();
            AttributeContainer.Clear();
            SpecialStatContainer.Clear();
            BlessingContainer.Clear();
            TraceContainer.Clear();
            ExtraContainer.Clear();
            RollsContainer.Clear();

            if (!_entityManager.TryGetComponent<LevelComponent>(CharaEntity, out var level))
            {
                Logger.WarningS("charsheet", $"entity {CharaEntity} does not posess a LevelComponent");
            }
            if (!_entityManager.TryGetComponent<CharaComponent>(CharaEntity, out var chara))
            {
                Logger.WarningS("charsheet", $"entity {CharaEntity} does not posess a CharaComponent");
            }
            if (!_entityManager.TryGetComponent<SkillsComponent>(CharaEntity, out var skills))
            {
                Logger.WarningS("charsheet", $"entity {CharaEntity} does not posess a SkillsComponent");
            }
            if (!_entityManager.TryGetComponent<CargoHolderComponent>(CharaEntity, out var cargoHold))
            {
                Logger.WarningS("charsheet", $"entity {CharaEntity} does not posess a CargoHolderComponent");
            }
            if (!_entityManager.TryGetComponent<WeightComponent>(CharaEntity, out var weight))
            {
                Logger.WarningS("charsheet", $"entity {CharaEntity} does not posess a WeightComponent");
            }
            if (!_entityManager.TryGetComponent<BuffsComponent>(CharaEntity, out var buffs))
            {
                Logger.WarningS("charsheet", $"entity {CharaEntity} does not posess a BuffsComponent");
            }

            var dict = new Dictionary<string, string>();
            if (chara != null)
            {
                dict[string.Empty] = string.Empty;
                dict[Loc.GetString("Elona.CharaSheet.Name")] = TempName;
                if (!string.IsNullOrEmpty(chara.Title))
                    dict[Loc.GetString("Elona.CharaSheet.Aka")] = chara.Title;
                dict[Loc.GetString("Elona.CharaSheet.Race")] = Loc.GetPrototypeString(chara.Race, "Name");
                dict[Loc.GetString("Elona.CharaSheet.Sex")] = Loc.GetString($"Elona.Gender.Names.{chara.Gender}.Normal").FirstCharToUpper();
                SetupContainer(NameContainer, 2, dict);
                dict.Clear();

                dict[string.Empty] = string.Empty;
                dict[Loc.GetString("Elona.CharaSheet.Class")] = Loc.GetPrototypeString(chara.Class, "Name");
                if (weight != null)
                {
                    dict[Loc.GetString("Elona.CharaSheet.Age")] = weight.Age.ToString();
                    dict[Loc.GetString("Elona.CharaSheet.Height")] = $"{weight.Height} {Loc.GetString("Elona.CharaSheet.Cm")}";
                    dict[Loc.GetString("Elona.CharaSheet.Weight")] = $"{weight.Weight} {Loc.GetString("Elona.CharaSheet.Kg")}";
                }
                SetupContainer(ClassContainer, 2, dict);
                dict.Clear();
            }
            if (level != null)
            {
                dict[Loc.GetString("Elona.CharaSheet.Level")] = level.Level.ToString();
                dict[Loc.GetString("Elona.CharaSheet.Exp")] = level.Experience.ToString();
                dict[Loc.GetString("Elona.CharaSheet.NextLv")] = level.ExperienceToNext.ToString();
            }
            dict[Loc.GetString("Elona.CharaSheet.God")] = TempGod;
            dict[Loc.GetString("Elona.CharaSheet.Guild")] = TempGuild;
            SetupContainer(ExpContainer, 2, dict);
            dict.Clear();


            AttributeContainer.AddElement(new UiTextTopic(Loc.GetString("Elona.CharaSheet.Topic.Attribute")));
            if (skills != null)
            {
                AttributeContainer.AddLayout(LayoutType.Spacer, 9);
                AttributeContainer.AddLayout(LayoutType.XOffset, 10);
                foreach (var attrProto in _skillsSys.EnumerateBaseAttributes())
                {
                    var cont = new UiHorizontalContainer();
                    var attrId = attrProto.GetStrongID();
                    if (!skills.Skills.TryGetValue(attrId, out var attrLvl))
                        continue;
                    string currentAmt = attrLvl.Level.Buffed.ToString();
                    string orgAmt = $"({attrLvl.Level.Base})";
                    var content = currentAmt
                        + new string(' ', Math.Max(1, AttributeSpacing - currentAmt.Length))
                        + orgAmt
                        + new string(' ', Math.Max(1, AttributePotentialSpacing - orgAmt.Length))
                        + CharaSheetHelpers.GetPotentialDescription(attrLvl.Potential);
                    cont.AddElement(new AttributeIcon(attrId));
                    cont.AddLayout(LayoutType.Spacer, 14);
                    cont.AddLayout(LayoutType.YOffset, -6);
                    cont.AddElement(MakeInfoContainer(Loc.GetPrototypeString(attrId, "ShortName"), 1, content));
                    AttributeContainer.AddElement(cont);
                }

                skills.Skills.TryGetValue(Protos.Skill.AttrLife, out var statLife);
                dict[Loc.GetPrototypeString(Protos.Skill.AttrLife, "Name")] = $"{statLife?.Level.Buffed}({statLife?.Level.Base})";
                skills.Skills.TryGetValue(Protos.Skill.AttrMana, out var statMana);
                dict[Loc.GetPrototypeString(Protos.Skill.AttrMana, "Name")] = $"{statMana?.Level.Buffed}({statMana?.Level.Base})";
                dict[Loc.GetString("Elona.CharaSheet.Sanity")] = TempSanity;
                skills.Skills.TryGetValue(Protos.Skill.AttrSpeed, out var statSpd);
                dict[Loc.GetPrototypeString(Protos.Skill.AttrSpeed, "Name")] = $"{statSpd?.Level.Buffed}({statSpd?.Level.Base})";
                dict[string.Empty] = string.Empty;
                dict[Loc.GetString("Elona.CharaSheet.Fame")] = TempFame;
                dict[Loc.GetString("Elona.CharaSheet.Karma")] = TempKarma;
                SetupContainer(SpecialStatContainer, 2, dict);
                dict.Clear();
            }
            BlessingContainer.AddElement(new UiTextTopic(Loc.GetString("Elona.CharaSheet.Topic.Blessing")));
            BlessingContainer.AddLayout(LayoutType.Spacer, 10);
            BlessingContainer.AddLayout(LayoutType.XOffset, 30);
            var blessCont = new UiGridContainer(GridType.Horizontal, 5, xCentered: false, xSpace: 8);
            for (int i = 0; i < 15; i++)
            {
                if (buffs?.Buffs.Count > i)
                    blessCont.AddElement(new HexAndBlessingIcon(buffs.Buffs[i]));
                else
                    blessCont.AddElement(new HexAndBlessingIcon(null));
            }
            BlessingContainer.AddElement(blessCont);

            TraceContainer.AddElement(new UiTextTopic(Loc.GetString("Elona.CharaSheet.Topic.Trace")));
            TraceContainer.AddLayout(LayoutType.Spacer, 6);
            TraceContainer.AddLayout(LayoutType.XOffset, 3);
            dict[Loc.GetString("Elona.CharaSheet.Turns")] = $"{_world.State.PlayTurns} {Loc.GetString("Elona.CharaSheet.TurnsPassed")}";
            dict[Loc.GetString("Elona.CharaSheet.Days")] = $"{_world.State.GameDate.Day - _world.State.InitialDate.Day} {Loc.GetString("Elona.CharaSheet.DaysPassed")}";
            dict[Loc.GetString("Elona.CharaSheet.Kills")] = TempKills;
            dict[Loc.GetString("Elona.CharaSheet.Time")] = _world.State.PlayTime.ToString();
            SetupContainer(TraceContainer, 2, dict);
            dict.Clear();

            ExtraContainer.AddElement(new UiTextTopic(Loc.GetString("Elona.CharaSheet.Topic.Extra")));
            ExtraContainer.AddLayout(LayoutType.Spacer, 6);
            ExtraContainer.AddLayout(LayoutType.XOffset, 3);

            if (cargoHold != null)
            {
                var cargoWeight = _cargoSys.GetTotalCargoWeight(CharaEntity);
                dict[Loc.GetString("Elona.CharaSheet.CargoWeight")] = UiUtils.DisplayWeight(cargoWeight);
                dict[Loc.GetString("Elona.CharaSheet.CargoLimit")] = UiUtils.DisplayWeight(cargoHold.MaxCargoWeight ?? 0);
            }
            var eqWeight = EquipmentHelpers.GetTotalEquipmentWeight(CharaEntity, _entityManager, _equipSlots);
            dict[Loc.GetString("Elona.CharaSheet.EquipWeight")] = $"{UiUtils.DisplayWeight(eqWeight)} {EquipmentHelpers.DisplayArmorClass(eqWeight)}";
            dict[Loc.GetString("Elona.CharaSheet.DeepestLevel")] = $"{_world.State.DeepestLevel}{Loc.GetString("Elona.CharaSheet.DeepestLevelDesc", ("level", _world.State.DeepestLevel))}";
            SetupContainer(ExtraContainer, 1, dict);
            dict.Clear();

            RollsContainer.AddElement(new UiTextTopic(Loc.GetString("Elona.CharaSheet.Topic.Rolls")));
        }

        private void SetupContainer(UiContainer cont, int xOffset, Dictionary<string, string> content)
        {
            var maxLen = content.Select(x => x.Key).Max(x => x.Length);
            foreach(var item in content)
            {
                cont.AddElement(MakeInfoContainer(item.Key, maxLen + xOffset, item.Value));
            }
        }

        private UiContainer MakeInfoContainer(string name, int xOffset, string content)
        {
            var cont = new UiHorizontalContainer();
            cont.AddElement(new UiText(UiFonts.CharSheetInfo, name 
                + (xOffset > 0 ? new string(' ', Math.Max(1, xOffset - name.Length)) : string.Empty)));
            cont.AddLayout(LayoutType.YOffset, -1);
            cont.AddElement(new UiText(UiFonts.CharaSheetInfoContent, content));
            return cont;
        }

        public override void GetPreferredSize(out Vector2i size)
        {
            size.X = SheetWidth;
            size.Y = SheetHeight;
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            FaceFrame.SetSize(90, 120);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            FaceFrame.SetPosition(x + 550, y + 25);

            NameContainer.SetPosition(x + 30, y + 47);
            NameContainer.Resolve();
            ClassContainer.SetPosition(x + 215, NameContainer.Y);
            ClassContainer.Resolve();
            ExpContainer.SetPosition(x + 355, NameContainer.Y);
            ExpContainer.Resolve();
            AttributeContainer.SetPosition(x + 27, y + 125);
            AttributeContainer.Resolve();
            SpecialStatContainer.SetPosition(x + 240, y + 155);
            SpecialStatContainer.Resolve();
            BlessingContainer.SetPosition(X + 400, AttributeContainer.Y);
            BlessingContainer.Resolve();
            TraceContainer.SetPosition(AttributeContainer.X, y + 285);
            TraceContainer.Resolve();
            ExtraContainer.SetPosition(X + 215, TraceContainer.Y);
            ExtraContainer.Resolve();
            RollsContainer.SetPosition(BlessingContainer.X, y + 260);
            RollsContainer.Resolve();
        }

        public override void Draw()
        {
            base.Draw();
            GraphicsEx.SetColor(0, 0, 0, 75);
            IeSheet.Draw(X + 4, Y + 4, SheetWidth, SheetHeight);
            GraphicsEx.SetColor(Color.White);
            IeSheet.Draw(X, Y, SheetWidth, SheetHeight);
            FaceFrame.Draw();
            NameContainer.Draw();
            ClassContainer.Draw();
            ExpContainer.Draw();
            AttributeContainer.Draw();
            SpecialStatContainer.Draw();
            BlessingContainer.Draw();
            TraceContainer.Draw();
            ExtraContainer.Draw();
            RollsContainer.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            FaceFrame.Update(dt);
            NameContainer.Update(dt);
            ClassContainer.Update(dt);
            ExpContainer.Update(dt);
            AttributeContainer.Update(dt);
            SpecialStatContainer.Update(dt);
            BlessingContainer.Update(dt);
            TraceContainer.Update(dt);
            ExtraContainer.Update(dt);
            RollsContainer.Update(dt);
        }
    }
}
