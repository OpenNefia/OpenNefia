using OpenNefia.Core.UI.Element;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Maths;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
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
using OpenNefia.Content.Equipment;
using OpenNefia.Content.Sanity;
using OpenNefia.Content.Fame;
using OpenNefia.Content.Karma;
using OpenNefia.Content.Guild;
using OpenNefia.Content.Religion;
using OpenNefia.Content.CustomName;
using OpenNefia.Content.DisplayName;
using OpenNefia.Core.UI;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.UserInterface;
using OpenNefia.Content.Input;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.CharaAppearance;
using OpenNefia.Core.Audio;
using System.Xml.Linq;
using OpenNefia.Content.Weight;
using OpenNefia.Content.Buffs;

namespace OpenNefia.Content.CharaInfo
{
    public class CharaSheetControl : UiElement
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

            private BuffPrototype? Buff;

            public HexAndBlessingIcon(BuffPrototype? buff)
            {
                TileIcon = Assets.Get(Protos.Asset.BuffIconNone);
                HexBlessingIcons = Assets.Get(Protos.Asset.BuffIcons);
                Buff = buff;
            }

            public override void GetPreferredSize(out Vector2 size)
            {
                size.X = TileWidth;
                size.Y = TileHeight;
            }

            public override void Draw()
            {
                base.Draw();

                var x = X + TileWidth / 2;
                var y = Y + TileHeight / 2;

                GraphicsEx.SetColor(255, 255, 255, 120);
                TileIcon.Draw(UIScale, x, y, centered: true);

                if (Buff != null)
                {
                    GraphicsEx.SetColor(Color.White);
                    HexBlessingIcons.DrawRegion(UIScale, Buff.RegionId, x, y);
                }
            }
        }

        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] protected readonly ISkillsSystem _skillsSys = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly ICargoSystem _cargoSys = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IPlayTimeManager _playTime = default!;
        [Dependency] private readonly IEquipmentSystem _equip = default!;

        private const int SheetWidth = 700;
        private const int SheetHeight = 400;
        private const int ContainerSpacing = 4;
        private const int AttributeContainerSpacing = 6;
        private const int AttributeSpacing = 5;
        private const int AttributePotentialSpacing = 8;

        private IAssetInstance AssetIeSheet;

        [Child] private CharaSheetFaceFrame FaceFrame;
        [Child] private UiContainer NameContainer;
        [Child] private UiContainer ClassContainer;
        [Child] private UiContainer ExpContainer;
        [Child] private UiContainer AttributeContainer;
        [Child] private UiContainer SpecialStatContainer;
        [Child] private UiContainer BlessingContainer;
        [Child] private UiContainer TraceContainer;
        [Child] private UiContainer ExtraContainer;
        [Child] private UiContainer RollsContainer;

        private UiContainer PlayTimeContainer = default!;
        private UiText TextPlayTime = default!;

        private EntityUid _charaEntity;

        private readonly LocaleScope _locScope;

        public CharaSheetControl()
        {
            EntitySystem.InjectDependencies(this);

            _locScope = Loc.MakeScope("Elona.CharaSheet");

            AssetIeSheet = Assets.Get(Asset.IeSheet);
            FaceFrame = new CharaSheetFaceFrame();

            NameContainer = new UiVerticalContainer { YSpace = ContainerSpacing };
            ClassContainer = new UiVerticalContainer { YSpace = ContainerSpacing };
            ExpContainer = new UiVerticalContainer { YSpace = ContainerSpacing };
            AttributeContainer = new UiVerticalContainer { YSpace = AttributeContainerSpacing };
            SpecialStatContainer = new UiVerticalContainer { YSpace = ContainerSpacing };
            BlessingContainer = new UiVerticalContainer();
            TraceContainer = new UiVerticalContainer { YSpace = ContainerSpacing };
            ExtraContainer = new UiVerticalContainer { YSpace = ContainerSpacing };
            RollsContainer = new UiVerticalContainer { YSpace = ContainerSpacing };

            EventFilter = UIEventFilterMode.Pass;
            CanControlFocus = true;
            OnKeyBindDown += HandleKeyBindDown;
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs evt)
{
            if (evt.Function == ContentKeyFunctions.UIPortrait)
            {
                var args = new CharaAppearanceLayer.Args(_charaEntity);
                UserInterfaceManager.Query<CharaAppearanceLayer, CharaAppearanceLayer.Args>(args);
                RefreshFromEntity();
                Sounds.Play(Sound.Chara);
            }
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            keyHints.Add(new(UiKeyHints.Portrait, ContentKeyFunctions.UIPortrait));

            return keyHints;
        }

        public void Initialize(EntityUid charaEntity)
        {
            _charaEntity = charaEntity;
        }

        public void RefreshFromEntity()
        {
            FaceFrame.RefreshFromEntity(_charaEntity);

            NameContainer.Clear();
            ClassContainer.Clear();
            ExpContainer.Clear();
            AttributeContainer.Clear();
            SpecialStatContainer.Clear();
            BlessingContainer.Clear();
            TraceContainer.Clear();
            ExtraContainer.Clear();
            RollsContainer.Clear();

            if (!_entityManager.TryGetComponent<LevelComponent>(_charaEntity, out var level))
            {
                Logger.WarningS("charasheet", $"entity {_charaEntity} does not posess a {nameof(LevelComponent)}");
            }
            if (!_entityManager.TryGetComponent<CharaComponent>(_charaEntity, out var chara))
            {
                Logger.WarningS("charasheet", $"entity {_charaEntity} does not posess a {nameof(CharaComponent)}");
            }
            if (!_entityManager.TryGetComponent<SkillsComponent>(_charaEntity, out var skills))
            {
                Logger.WarningS("charasheet", $"entity {_charaEntity} does not posess a {nameof(SkillsComponent)}");
            }
            if (!_entityManager.TryGetComponent<CargoHolderComponent>(_charaEntity, out var cargoHold))
            {
                Logger.WarningS("charasheet", $"entity {_charaEntity} does not posess a {nameof(CargoHolderComponent)}");
            }
            if (!_entityManager.TryGetComponent<WeightComponent>(_charaEntity, out var weight))
            {
                Logger.WarningS("charasheet", $"entity {_charaEntity} does not posess a {nameof(WeightComponent)}");
            }
            if (!_entityManager.TryGetComponent<BuffsComponent>(_charaEntity, out var buffs))
            {
                Logger.WarningS("charasheet", $"entity {_charaEntity} does not posess a {nameof(BuffsComponent)}");
            }
            if (!_entityManager.TryGetComponent<SanityComponent>(_charaEntity, out var sanity))
            {
                Logger.WarningS("charasheet", $"entity {_charaEntity} does not posess a {nameof(SanityComponent)}");
            }
            if (!_entityManager.TryGetComponent<FameComponent>(_charaEntity, out var fame))
            {
                Logger.WarningS("charasheet", $"entity {_charaEntity} does not posess a {nameof(FameComponent)}");
            }
            if (!_entityManager.TryGetComponent<KarmaComponent>(_charaEntity, out var karma))
            {
                Logger.WarningS("charasheet", $"entity {_charaEntity} does not posess a {nameof(KarmaComponent)}");
            }
            if (!_entityManager.TryGetComponent<GuildMemberComponent>(_charaEntity, out var guild))
            {
                Logger.WarningS("charasheet", $"entity {_charaEntity} does not posess a {nameof(GuildMemberComponent)}");
            }
            if (!_entityManager.TryGetComponent<ReligionComponent>(_charaEntity, out var god))
            {
                Logger.WarningS("charasheet", $"entity {_charaEntity} does not posess a {nameof(ReligionComponent)}");
            }
            if (!_entityManager.TryGetComponent<CustomNameComponent>(_charaEntity, out var customName))
            {
                Logger.WarningS("charasheet", $"entity {_charaEntity} does not posess a {nameof(CustomNameComponent)}");
            }

            var dict = new OrderedDictionary<string, string>();

            //
            // Personal
            //

            if (chara != null)
            {
                dict[string.Empty] = string.Empty;
                dict[_locScope.GetString("Group.Personal.Name")] = _displayNames.GetBaseName(_charaEntity);
                if (!string.IsNullOrEmpty(chara.Alias))
                    dict[_locScope.GetString("Group.Personal.Alias")] = chara.Alias;
                dict[_locScope.GetString("Group.Personal.Race")] = Loc.GetPrototypeString(chara.Race, "Name");
                dict[_locScope.GetString("Group.Personal.Sex")] = Loc.GetString($"Elona.Gender.Names.{chara.Gender}.Normal").FirstCharToUpper();
                SetupContainer(NameContainer, 4, dict);
                dict.Clear();

                dict[string.Empty] = string.Empty;
                dict[_locScope.GetString("Group.Personal.Class")] = Loc.GetPrototypeString(chara.Class, "Name");
                if (weight != null)
                {
                    dict[_locScope.GetString("Group.Personal.Age")] = _locScope.GetString("Group.Personal.AgeCounter", ("years", weight.Age));
                    dict[_locScope.GetString("Group.Personal.Height")] = $"{weight.Height} {_locScope.GetString("Group.Personal.Cm")}";
                    dict[_locScope.GetString("Group.Personal.Weight")] = $"{weight.Weight} {_locScope.GetString("Group.Personal.Kg")}";
                }
                SetupContainer(ClassContainer, 4, dict);
                dict.Clear();
            }

            //
            // Level
            //

            var levelGodId = god?.GodID;
            var levelGodName = levelGodId != null
                ? Loc.GetPrototypeString(levelGodId.Value, "Name")
                : Loc.GetString("Elona.God.Eyth.Name");

            var levelGuildId = guild?.GuildID;
            var levelGuildName = levelGuildId != null
                ? Loc.GetPrototypeString(levelGuildId.Value, "Name")
                : Loc.GetString("Elona.Guild.Name.None");

            if (level != null)
            {
                dict[_locScope.GetString("Group.Exp.Level")] = level.Level.ToString();
                dict[_locScope.GetString("Group.Exp.Exp")] = level.Experience.ToString();
                dict[_locScope.GetString("Group.Exp.RequiredExp")] = level.ExperienceToNext.ToString();
            }
            dict[_locScope.GetString("Group.Exp.God")] = levelGodName;
            dict[_locScope.GetString("Group.Exp.Guild")] = levelGuildName;
            SetupContainer(ExpContainer, 5, dict);
            dict.Clear();

            //
            // Attributes
            //

            AttributeContainer.AddElement(new UiTextTopic(_locScope.GetString("Topic.Attribute")));
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
                    cont.AddElement(MakeInfoContainer(Loc.GetPrototypeString(attrId, "ShortName"), 5, content));

                    AttributeContainer.AddElement(cont);
                }

                var attributeInsanity = sanity != null ? sanity.Insanity : 0;
                var attributeFame = fame != null ? fame.Fame.Buffed : 0;
                var attributeKarma = karma != null ? karma.Karma.Buffed : 0;

                skills.Skills.TryGetValue(Protos.Skill.AttrLife, out var statLife);
                dict[Loc.GetPrototypeString(Protos.Skill.AttrLife, "Name")] = $"{statLife?.Level.Buffed}({statLife?.Level.Base})";
                skills.Skills.TryGetValue(Protos.Skill.AttrMana, out var statMana);
                dict[Loc.GetPrototypeString(Protos.Skill.AttrMana, "Name")] = $"{statMana?.Level.Buffed}({statMana?.Level.Base})";
                dict[_locScope.GetString("Group.Attribute.Sanity")] = attributeInsanity.ToString();
                skills.Skills.TryGetValue(Protos.Skill.AttrSpeed, out var statSpd);
                dict[Loc.GetPrototypeString(Protos.Skill.AttrSpeed, "Name")] = $"{statSpd?.Level.Buffed}({statSpd?.Level.Base})";
                dict[string.Empty] = string.Empty;
                dict[_locScope.GetString("Group.Attribute.Fame")] = attributeFame.ToString();
                dict[_locScope.GetString("Group.Attribute.Karma")] = attributeKarma.ToString();
                SetupContainer(SpecialStatContainer, 6, dict);
                dict.Clear();
            }

            //
            // Blessing
            //

            BlessingContainer.AddElement(new UiTextTopic(_locScope.GetString("Topic.Blessing")));
            BlessingContainer.AddLayout(LayoutType.Spacer, 10);
            BlessingContainer.AddLayout(LayoutType.XOffset, 30);
            var blessCont = new UiGridContainer(GridType.Horizontal, 5, xCentered: false, xSpace: 8);
            for (int i = 0; i < 15; i++)
            {
                if (buffs?.Buffs.Count > i)
                    blessCont.AddElement(new HexAndBlessingIcon(_protos.Index(buffs.Buffs[i].BuffID)));
                else
                    blessCont.AddElement(new HexAndBlessingIcon(null));
            }
            BlessingContainer.AddElement(blessCont);

            //
            // Trace
            //

            var traceDays = _world.State.GameDate.Day - _world.State.InitialDate.Day;

            TraceContainer.AddElement(new UiTextTopic(_locScope.GetString("Topic.Trace")));
            TraceContainer.AddLayout(LayoutType.Spacer, 6);
            TraceContainer.AddLayout(LayoutType.XOffset, 3);
            dict[_locScope.GetString("Group.Trace.Turns")] = $"{_locScope.GetString("Group.Trace.TurnsCounter", ("turns", _world.State.PlayTurns))}";
            dict[_locScope.GetString("Group.Trace.Days")] = $"{_locScope.GetString("Group.Trace.DaysCounter", ("days", traceDays))}";
            dict[_locScope.GetString("Group.Trace.Kills")] = _world.State.TotalKills.ToString();
            dict[_locScope.GetString("Group.Trace.Time")] = string.Empty;
            SetupContainer(TraceContainer, 4, dict);
            PlayTimeContainer = (UiContainer)TraceContainer.Entries[6].Element!;
            TextPlayTime = (UiText)PlayTimeContainer.Entries[2].Element!;
            dict.Clear();

            //
            // Extra
            //

            ExtraContainer.AddElement(new UiTextTopic(_locScope.GetString("Topic.Extra")));
            ExtraContainer.AddLayout(LayoutType.Spacer, 6);
            ExtraContainer.AddLayout(LayoutType.XOffset, 3);

            if (cargoHold != null)
            {
                var cargoWeight = _cargoSys.GetTotalCargoWeight(_charaEntity);
                dict[_locScope.GetString("Group.Extra.CargoWeight")] = UiUtils.DisplayWeight(cargoWeight);
                dict[_locScope.GetString("Group.Extra.CargoLimit")] = UiUtils.DisplayWeight(cargoHold.MaxCargoWeight ?? 0);
            }
            var eqWeight = _equip.GetTotalEquipmentWeight(_charaEntity);
            dict[_locScope.GetString("Group.Extra.EquipWeight")] = $"{UiUtils.DisplayWeight(eqWeight)} {EquipmentHelpers.DisplayArmorClass(eqWeight)}";
            dict[_locScope.GetString("Group.Extra.DeepestLevel")] = $"{_locScope.GetString("Group.Extra.DeepestLevelCounter", ("level", _world.State.DeepestLevel))}";
            SetupContainer(ExtraContainer, 6, dict);
            dict.Clear();

            //
            // Rolls
            //

            RollsContainer.AddElement(new UiTextTopic(_locScope.GetString("Topic.Rolls")));

            NameContainer.Relayout();
            ClassContainer.Relayout();
            ExpContainer.Relayout();
            AttributeContainer.Relayout();
            SpecialStatContainer.Relayout();
            BlessingContainer.Relayout();
            TraceContainer.Relayout();
            ExtraContainer.Relayout();
            RollsContainer.Relayout();
        }

        private void SetupContainer(UiContainer cont, int xOffset, IDictionary<string, string> content)
        {
            var maxLen = content.Select(x => x.Key).Max(x => x.Length);
            foreach (var item in content)
            {
                cont.AddElement(MakeInfoContainer(item.Key, maxLen + xOffset, item.Value));
            }
        }

        private UiContainer MakeInfoContainer(string name, int xOffset, string content)
        {
            var cont = new UiHorizontalContainer();
            cont.AddElement(new UiText(UiFonts.CharaSheetInfo, name.WidePadRight(xOffset)));
            cont.AddLayout(LayoutType.YOffset, -1);
            cont.AddElement(new UiText(UiFonts.CharaSheetInfoContent, content));
            return cont;
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            size.X = SheetWidth;
            size.Y = SheetHeight;
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            FaceFrame.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            FaceFrame.SetPosition(x + 558, y + 23);

            NameContainer.SetPosition(x + 30, y + 47);
            NameContainer.Relayout();
            ClassContainer.SetPosition(x + 215, NameContainer.Y);
            ClassContainer.Relayout();
            ExpContainer.SetPosition(x + 355, NameContainer.Y);
            ExpContainer.Relayout();
            AttributeContainer.SetPosition(x + 27, y + 125);
            AttributeContainer.Relayout();
            SpecialStatContainer.SetPosition(x + 240, y + 155);
            SpecialStatContainer.Relayout();
            BlessingContainer.SetPosition(X + 400, AttributeContainer.Y);
            BlessingContainer.Relayout();
            TraceContainer.SetPosition(AttributeContainer.X, y + 285);
            TraceContainer.Relayout();
            ExtraContainer.SetPosition(X + 215, TraceContainer.Y);
            ExtraContainer.Relayout();
            RollsContainer.SetPosition(BlessingContainer.X, y + 260);
            RollsContainer.Relayout();
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

            TextPlayTime.Text = _playTime.PrecisePlayTime.ToString(@"hh\:mm\:ss");
        }

        public override void Draw()
        {
            base.Draw();
            GraphicsEx.SetColor(0, 0, 0, 75);
            AssetIeSheet.DrawUnscaled(PixelX + 4, PixelY + 4, SheetWidth * UIScale, SheetHeight * UIScale);
            GraphicsEx.SetColor(Color.White);
            AssetIeSheet.DrawUnscaled(PixelX, PixelY, SheetWidth * UIScale, SheetHeight * UIScale);
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
    }
}
