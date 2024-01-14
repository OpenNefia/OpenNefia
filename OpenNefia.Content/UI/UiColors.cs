using OpenNefia.Core.Maths;

namespace OpenNefia.Content.UI
{
    public static class UiColors
    {
        public static readonly Color TextWhite = new(255, 255, 255, 255);
        public static readonly Color TextBlack = new(0, 0, 0, 255);
        public static readonly Color TextDisabled = new(0, 0, 0, 128);
        public static readonly Color TextKeyHintBar = new(250, 250, 250, 255);
        public static readonly Color TextAutoTurn = new(235, 235, 235, 255);

        public static readonly Color ListSelectedAdd = new(50, 50, 50, 255);
        public static readonly Color ListSelectedSub = new(30, 10, 0, 255);
        public static readonly Color ListEntryAccent = new(12, 14, 16, 16);

        public static readonly Color WindowBottomLine1 = new(194, 170, 146, 255);
        public static readonly Color WindowBottomLine2 = new(234, 220, 188, 255);

        public static readonly Color ScrollBottomLine1 = new(194, 173, 161, 255);
        public static readonly Color ScrollBottomLine2 = new(224, 213, 191, 255);

        public static readonly Color TopicWindowStyle0 = new(255, 255, 255, 255);
        public static readonly Color TopicWindowStyle1 = new(60, 50, 60, 255);
        public static readonly Color TopicWindowStyle2 = new(45, 40, 50, 255);
        public static readonly Color TopicWindowStyle3 = new(245, 242, 239, 255);
        public static readonly Color TopicWindowStyle4 = new(60, 50, 60, 255);
        public static readonly Color TopicWindowStyle5 = new(195, 205, 195, 255);
        public static readonly Color TopicWindowStyle6 = new(255, 255, 255, 180);

        public static readonly Color PromptBackground = new(0, 0, 0, 127);
        public static readonly Color PromptTargetedTile = new(127, 127, 255, 50);
        public static readonly Color PromptHighlightedTile = new Color(255, 255, 255, 25);

        public static readonly Color RandomEventPromptTitle = new(30, 20, 10);
        public static readonly Color RandomEventPromptBody = new(30, 30, 30);

        public static readonly Color ReplBackground = new(17, 65, 17, 192);
        public static readonly Color ReplText = new(255, 255, 255, 255);
        public static readonly Color ReplTextResult = new(150, 200, 200, 255);
        public static readonly Color ReplTextError = new(255, 0, 0, 255);
        public static readonly Color ReplCompletionBorder = new(240, 240, 240, 255);
        public static readonly Color ReplCompletionBackground = new(40, 40, 40, 255);
        public static readonly Color ReplCompletionExactMatchBackground = new(120, 120, 40, 255);

        public static readonly Color MesWhite = new(255, 255, 255, 255);
        public static readonly Color MesGreen = new(175, 255, 175, 255);
        public static readonly Color MesRed = new(255, 155, 155, 255);
        public static readonly Color MesBlue = new(175, 175, 255, 255);
        public static readonly Color MesBrown = new(255, 215, 175, 255);
        public static readonly Color MesYellow = new(255, 255, 175, 255);
        public static readonly Color MesBlack = new(155, 154, 153, 255);
        public static readonly Color MesPurple = new(185, 155, 215, 255);
        public static readonly Color MesSkyBlue = new(155, 205, 205, 255);
        public static readonly Color MesPink = new(255, 195, 185, 255);
        public static readonly Color MesOrange = new(235, 215, 155, 255);
        public static readonly Color MesFresh = new(225, 215, 185, 255);
        public static readonly Color MesDarkGreen = new(105, 235, 105, 255);
        public static readonly Color MesGray = new(205, 205, 205, 255);
        public static readonly Color MesLightRed = new(255, 225, 225, 255);
        public static readonly Color MesLightBlue = new(225, 225, 255, 255);
        public static readonly Color MesLightPurple = new(225, 195, 255, 255);
        public static readonly Color MesLightGreen = new(215, 255, 215, 255);
        public static readonly Color MesTalk = new(210, 250, 160, 255);

        public static readonly Color CharaMakeAttrLevelNone = new(120, 120, 120);
        public static readonly Color CharaMakeAttrLevelSlight = new(200, 0, 0);
        public static readonly Color CharaMakeAttrLevelLittle = new(150, 0, 0);
        public static readonly Color CharaMakeAttrLevelNormal = new(0, 0, 0);
        public static readonly Color CharaMakeAttrLevelNotBad = new(0, 0, 150);
        public static readonly Color CharaMakeAttrLevelGood = new(0, 0, 150);
        public static readonly Color CharaMakeAttrLevelGreat = new(0, 0, 200);
        public static readonly Color CharaMakeAttrLevelBest = new(0, 0, 200);

        public static readonly Color CharaSheetSelectedBuff = new(200, 200, 225, 63);

        public static readonly Color InventoryItemNoDrop = new(120, 80, 0, 255);
        public static readonly Color InventoryItemDoomed = new(100, 10, 100, 255);
        public static readonly Color InventoryItemCursed = new(150, 10, 10, 255);
        public static readonly Color InventoryItemNormal = new(10, 40, 120, 255);
        public static readonly Color InventoryItemBlessed = new(10, 110, 30, 255);

        public static readonly Color EquipmentItemTextDefault = new Color(10, 10, 10, 255);

        public static readonly Color EnchantmentDefault = new Color(80, 50, 0, 255);
        public static readonly Color EnchantmentNegative = new Color(180, 0, 0, 255);

        public static readonly Color HungerIndicatorHungry = new Color(200, 0, 0, 255);
        public static readonly Color HungerIndicatorStarving = new Color(250, 0, 0, 255);

        public static readonly Color FatigueIndicatorLight = new Color(60, 60, 0, 255);
        public static readonly Color FatigueIndicatorModerate = new Color(80, 80, 0, 255);
        public static readonly Color FatigueIndicatorHeavy = new Color(120, 120, 0, 255);

        public static readonly Color SleepIndicatorLight = new Color(0, 0, 0, 255);
        public static readonly Color SleepIndicatorModerate = new Color(100, 100, 0, 255);
        public static readonly Color SleepIndicatorHeavy = new Color(255, 0, 0, 255);

        public static readonly Color DialogText = new Color(20, 10, 5, 255);

        public static readonly Color QuestDifficultyVeryHigh = new Color(205, 0, 0);
        public static readonly Color QuestDifficultyHigh = new Color(140, 80, 0);
        public static readonly Color QuestDifficultyModerate = new Color(0, 0, 205);
        public static readonly Color QuestDifficultyEasy = new Color(0, 155, 0);

        public static readonly Color HouseBoardRankStar = new Color(255, 255, 50);

        public static readonly Color SceneTextFg = new Color(240, 240, 240);
        public static readonly Color SceneTextBg = new Color(10, 10, 10);
    }
}
