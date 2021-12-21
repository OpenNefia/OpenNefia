﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Core.UI
{
    public static class UiFonts
    {
        public static readonly FontSpec TitleScreenText = new(13, 13, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);

        public static readonly FontSpec ListTitleScreenText = new(13, 14);
        public static readonly FontSpec ListTitleScreenSubtext = new(11, 11);
        public static readonly FontSpec ListText = new(14, 12);
        public static readonly FontSpec ListKeyName = new(15, 13, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);

        public static readonly FontSpec WindowTitle = new(15, 14, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);
        public static readonly FontSpec WindowKeyHints = new(12, 10);

        public static readonly FontSpec PromptText = new(16, 14, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);
        public static readonly FontSpec TargetText = new(14, 12, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);

        public static readonly FontSpec ReplText = new(13, 13, color: UiColors.ReplText);
        public static readonly FontSpec ReplCompletion = new(13, 13, color: UiColors.ReplText);

        public static readonly FontSpec FpsCounter = new(14, 14, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);
    }
}
