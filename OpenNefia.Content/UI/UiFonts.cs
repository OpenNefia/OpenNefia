using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.UI
{
    public static class UiFonts
    {
        public static readonly FontSpec TitleScreenText = new(13, 13, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);

        public static readonly FontSpec ListTitleScreenText = new(14, 13);
        public static readonly FontSpec ListTitleScreenSubtext = new(11, 11);
        public static readonly FontSpec ListText = new(14, 12);
        public static readonly FontSpec ListKeyName = new(15, 13, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);

        public static readonly FontSpec WindowTitle = new(15, 14, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);
        public static readonly FontSpec WindowKeyHints = new(12, 10);
        public static readonly FontSpec WindowPage = new(12, 12, style: FontStyle.Bold);

        public static readonly FontSpec CharaMakeLockInfo = new(12, 10);
        public static readonly FontSpec CharaMakeLockInfoBold = new(12, 10, style: FontStyle.Bold);
        public static readonly FontSpec CharaMakeRerollAttrAmount = new(14, 12, style: FontStyle.Bold);
        public static readonly FontSpec CharaMakeRerollLocked = new(11, 9, color: UiColors.CharaMakeAttrLevelGreat, style: FontStyle.Bold);

        public static readonly FontSpec CharaSheetInfo = new(13, 11, style: FontStyle.Bold);
        public static readonly FontSpec CharaSheetInfoContent = new(14, 12);
        public static readonly FontSpec KeyHintBar = new(12, 12, color: UiColors.TextKeyHintBar, bgColor: UiColors.TextBlack);

        public static readonly FontSpec SkillsListHeader = new(12, 10, style: FontStyle.Bold);
        public static readonly FontSpec SkillsListBonusPoints = new(12, 10, style: FontStyle.Bold);

        public static readonly FontSpec TextNote = new(12, 10, style: FontStyle.Bold); // 12 + sizefix - en * 2

        public static readonly FontSpec MessageText = new(14, 14);
        public static readonly FontSpec HUDTabText = new(14, 12, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);
        public static readonly FontSpec HUDBarText = new(12, 10, color: UiColors.TextWhite, bgColor: UiColors.TextBlack, style: FontStyle.Bold);
        public static readonly FontSpec HUDInfoText = new(13, 11, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);
        public static readonly FontSpec HUDSkillText = new(13, 11);
        public static readonly FontSpec HUDAutoTurnText = new(13, 13, color: UiColors.TextAutoTurn, bgColor: UiColors.TextBlack);

        public static readonly FontSpec PromptText = new(16, 14, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);
        public static readonly FontSpec TargetText = new(14, 12, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);
        public static readonly FontSpec RandomEventPromptTitle = new(14, 14, color: UiColors.TextWhite, bgColor: UiColors.RandomEventPromptTitle);
        public static readonly FontSpec RandomEventPromptBody = new(14, 14, color: UiColors.RandomEventPromptBody);

        public static readonly FontSpec ReplText = new(13, 13, color: UiColors.ReplText);
        public static readonly FontSpec ReplCompletion = new(13, 13, color: UiColors.ReplText);

        public static readonly FontSpec FpsCounter = new(14, 14, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);

        public static readonly FontSpec InventoryGoldCount = new(13, 11);

        public static readonly FontSpec ItemDescNormal = new(14, 12);
        public static readonly FontSpec ItemDescFlavor = new(13, 11);
        public static readonly FontSpec ItemDescFlavorItalic = new(13, 11, style: FontStyle.Italic);

        public static readonly FontSpec EquipmentEquipSlotName = new(12, 10, style: FontStyle.Bold); // 12 + sizefix - en * 2

        public static readonly FontSpec StatusIndicatorText = new(13, 11); // 13 - en * 2

        public static readonly FontSpec DialogSpeakerName = new(12, 12, style: FontStyle.Bold, color: UiColors.DialogText); // 12 + sizefix
        public static readonly FontSpec DialogImpressionText = new(13, 13, color: UiColors.DialogText);
        public static readonly FontSpec DialogBodyText = new(14, 14, color: UiColors.DialogText);

        public static readonly FontSpec QuestBoardDifficultyNormal = new(14, 12);
        public static readonly FontSpec QuestBoardDifficultySmall = new(10, 10);
        public static readonly FontSpec QuestBoardPage = new(16, 16, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);

        public static readonly FontSpec HouseBoardRankStar = new(14, 14, color: UiColors.HouseBoardRankStar, bgColor: UiColors.TextBlack);
        public static readonly FontSpec HouseBoardRankText = new(12, 12, color: UiColors.TextBlack); // 12 + sizefix
    }
}
