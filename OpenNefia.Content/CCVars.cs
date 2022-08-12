using OpenNefia.Core.Configuration;
using OpenNefia.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Activity;

namespace OpenNefia.Content
{   
    /// <summary>
    /// Contains content <see cref="CVar"/>s.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    [CVarDefs]
    public sealed class CCVars : CVars
    {
        /*
         * Game
         */

        public static readonly CVarDef<bool> GameEnableCutscenes =
            CVarDef.Create("game.enableCutscenes", true, CVar.Archive);

        public static readonly CVarDef<string> GameDefaultSaveID =
            CVarDef.Create("game.defaultSaveID", string.Empty, CVar.Archive);

        public static readonly CVarDef<bool> GameAttackNeutralNPCs =
            CVarDef.Create("game.attackNeutralNPCs", false, CVar.Archive);

        public static readonly CVarDef<ShopUpdatesType> GameHideShopUpdates =
            CVarDef.Create("game.hideShopUpdates", ShopUpdatesType.None, CVar.Archive);

        public static readonly CVarDef<AutoIdentifyType> GameHideAutoidentify =
            CVarDef.Create("game.hideAutoidentify", AutoIdentifyType.None, CVar.Archive);

        public static readonly CVarDef<bool> GameExtraHelp =
            CVarDef.Create("game.extraHelp", true, CVar.Archive);

        public static readonly CVarDef<bool> GameSkipRandomEventPopups =
            CVarDef.Create("game.skipRandomEventPopups", false, CVar.Archive);

        public static readonly CVarDef<AutosaveType> GameAutosave =
            CVarDef.Create("game.autosave", AutosaveType.Always, CVar.Archive);

        public static readonly CVarDef<SaveOnReturnToTitle> GameSaveOnReturnToTitle =
            CVarDef.Create("game.saveOnReturnToTitle", SaveOnReturnToTitle.Never, CVar.Archive);

        public static readonly CVarDef<bool> GameItemShortcutsRespectCurseState =
            CVarDef.Create("game.itemShortcutsRespectCurseState", false, CVar.Archive);

        public static readonly CVarDef<bool> GameWarnOnSpellOvercast =
            CVarDef.Create("game.warnOnSpellOvercast", true, CVar.Archive);

        /*
         * Display
         */

        public static readonly CVarDef<bool> DisplayObjectShadows =
            CVarDef.Create("display.objectShadows", true, CVar.Archive);

        public static readonly CVarDef<bool> DisplayHighQualityShadows =
            CVarDef.Create("display.highQualityShadows", true, CVar.Archive);

        public static readonly CVarDef<bool> DisplayHeartbeat =
            CVarDef.Create("display.heartbeat", true, CVar.Archive);

        public static readonly CVarDef<float> DisplayHeartbeatThreshold =
            CVarDef.Create("display.heartbeatThreshold", 0.2f, CVar.Archive);

        public static readonly CVarDef<bool> DisplayWeatherEffects =
            CVarDef.Create("display.weatherEffects", true, CVar.Archive);

        /*
         * Net
         */

        public static readonly CVarDef<bool> NetEnable =
            CVarDef.Create("net.enable", true, CVar.Archive);

        /*
         * Anime
         */

        public static readonly CVarDef<bool> AnimeAttackAnimation =
            CVarDef.Create("anime.attackAnimation", true, CVar.Archive);

        public static readonly CVarDef<int> AnimeGeneralWait =
            CVarDef.Create("anime.generalWait", 2, CVar.Archive);

        public static readonly CVarDef<bool> AnimeTitleEffect =
            CVarDef.Create("anime.titleEffect", true, CVar.Archive);

        public static readonly CVarDef<AutoTurnSpeed> AnimeAutoTurnSpeed =
            CVarDef.Create("anime.autoTurnSpeed", AutoTurnSpeed.Normal, CVar.Archive);

        public static readonly CVarDef<ScrollTargets> AnimeScroll =
            CVarDef.Create("anime.scroll", ScrollTargets.None, CVar.Archive);

        public static readonly CVarDef<ScrollType> AnimeScrollType =
            CVarDef.Create("anime.scrollType", ScrollType.Smooth, CVar.Archive);

        public static readonly CVarDef<bool> AnimeWindowAnimation =
            CVarDef.Create("anime.windowAnimation", false, CVar.Archive);

        public static readonly CVarDef<bool> AnimeAlwaysCenter =
            CVarDef.Create("anime.alwaysCenter", true, CVar.Archive);

        public static readonly CVarDef<float> AnimeAlertWait =
            CVarDef.Create("anime.alertWait", 0.5f, CVar.Archive);

        public static readonly CVarDef<int> AnimeScreenRefresh =
            CVarDef.Create("anime.screenRefresh", 20, CVar.Archive);

        public static readonly CVarDef<float> AnimeAnimationWait =
            CVarDef.Create("anime.animationWait", 0.2f, CVar.Archive);

        public static readonly CVarDef<AnimeWaitType> AnimeAnimationWaitType =
            CVarDef.Create("anime.animationWaitType", AnimeWaitType.AlwaysWait, CVar.Archive);

        public static readonly CVarDef<float> AnimeBackgroundEffectWait =
            CVarDef.Create("anime.backgroundEffectWait", 0.3f, CVar.Archive);

        public static readonly CVarDef<bool> AnimeScrollWhenRun =
            CVarDef.Create("anime.scrollWhenRun", true, CVar.Archive);

        public static readonly CVarDef<bool> AnimeSkipSleepAnimation =
            CVarDef.Create("anime.skipSleepAnimation", false, CVar.Archive);

        public static readonly CVarDef<bool> AnimeSkipFishingAnimation =
            CVarDef.Create("anime.skipFishingAnimation", false, CVar.Archive);

        /*
         * Input
         */

        public static readonly CVarDef<int> InputAttackWait =
            CVarDef.Create("input.attackWait", 4, CVar.Archive);

        public static readonly CVarDef<int> InputKeyRepeatWait =
            CVarDef.Create("input.keyRepeatWait", 1, CVar.Archive);

        public static readonly CVarDef<int> InputInitialKeyRepeatWait =
            CVarDef.Create("input.initialKeyRepeatWait", 5, CVar.Archive);

        public static readonly CVarDef<int> InputSelectWait =
            CVarDef.Create("input.selectWait", 5, CVar.Archive);

        public static readonly CVarDef<int> InputRunWait =
            CVarDef.Create("input.runWait", 2, CVar.Archive);

        public static readonly CVarDef<int> InputStartRunWait =
            CVarDef.Create("input.startRunWait", 2, CVar.Archive);

        public static readonly CVarDef<int> InputWalkWait =
            CVarDef.Create("input.walkWait", 5, CVar.Archive);

        public static readonly CVarDef<int> InputKeyWait =
            CVarDef.Create("input.keyWait", 5, CVar.Archive);

        /*
         * Message
         */

        public static readonly CVarDef<bool> MessageTimestamps =
            CVarDef.Create("message.timestamps", false, CVar.Archive);

        public static readonly CVarDef<int> MessageFade =
            CVarDef.Create("message.fade", 50, CVar.Archive);

        public static readonly CVarDef<DisplayDamageType> MessageShowDamageNumbers =
            CVarDef.Create("message.showDamageNumbers", DisplayDamageType.Always, CVar.Archive);

        /*
         * Debug
         */

        public static readonly CVarDef<bool> DebugQuickstartOnStartup =
            CVarDef.Create("debug.quickstartOnStartup", true, CVar.Archive);

        public static readonly CVarDef<bool> DebugShowDetailedSkillPower =
            CVarDef.Create("debug.showDetailedSkillPower", false, CVar.Archive);

        public static readonly CVarDef<bool> DebugShowDetailedResistPower =
            CVarDef.Create("debug.showDetailedResistPower", false, CVar.Archive);

        public static readonly CVarDef<bool> DebugShowImpression =
            CVarDef.Create("debug.showImpression", false, CVar.Archive | CVar.Cheat);

        public static readonly CVarDef<bool> DebugUnlimitedSkillPoints =
            CVarDef.Create("debug.unlimitedSkillPoints", false, CVar.Archive | CVar.Cheat);

        public static readonly CVarDef<bool> DebugSkipRandomEvents =
            CVarDef.Create("debug.skipRandomEvents", false, CVar.Archive | CVar.Cheat);

        public static readonly CVarDef<ForceMapRenewalType> DebugForceMapRenewal =
            CVarDef.Create("debug.forceMapRenewal", ForceMapRenewalType.Disabled, CVar.Archive);

        public static readonly CVarDef<bool> DebugLivingWeapon =
            CVarDef.Create("debug.livingWeapon", false, CVar.Archive);

        public static readonly CVarDef<bool> DebugAlwaysDropFigureAndCard =
            CVarDef.Create("debug.alwaysDropFigureAndCard", false, CVar.Archive | CVar.Cheat);

        public static readonly CVarDef<bool> DebugAlwaysDropRemains =
            CVarDef.Create("debug.alwaysDropRemains", false, CVar.Archive | CVar.Cheat);

        public static readonly CVarDef<bool> DebugProductionVersatileTool =
            CVarDef.Create("debug.productionVersatileTool", false, CVar.Archive | CVar.Cheat);

        public static readonly CVarDef<bool> DebugNoEncounters =
            CVarDef.Create("debug.noEncounters", false, CVar.Archive | CVar.Cheat);

        public static readonly CVarDef<bool> DebugAutoIdentify =
            CVarDef.Create("debug.autoIdentify", false, CVar.Archive | CVar.Cheat);
    }

    public enum AutosaveType
    {
        Always,
        Sometimes,
        Rarely,
        AlmostNever
    }

    public enum SaveOnReturnToTitle
    {
        Always,
        Ask,
        Never,
    }

    public enum ShopUpdatesType
    {
        None,
        CouldNotSell,
        All
    }

    public enum AutoIdentifyType
    {
        None,
        Quality,
        All
    }

    public enum ScrollTargets
    {
        None,
        Player,
        All
    }

    public enum ScrollType
    {
        Smooth,
        Classic
    }

    public enum AnimeWaitType
    {
        AlwaysWait,
        AtTurnStart,
        NeverWait
    }
    
    public enum ForceMapRenewalType
    {
        Disabled,
        Minor,
        Major
    }

    public enum DisplayDamageType
    {
        Never,
        SandbagOnly,
        Always
    }
}
