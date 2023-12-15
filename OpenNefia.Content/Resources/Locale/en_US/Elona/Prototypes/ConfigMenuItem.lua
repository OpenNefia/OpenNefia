OpenNefia.Prototypes.Elona.ConfigMenuItem.Elona = {
    MenuDefault = {
        Name = "Option",
    },

    --
    -- Menu: Game
    --

    MenuGame = {
        Name = "Game Setting",
    },

    --
    -- Menu: Screen
    --

    MenuScreen = {
        Name = "Screen & Sound",
    },

    ScreenMusic = {
        Description = "Enable or disable music.",
        Name = "Music",
        YesNo = "config.common.yes_no.on_off",
    },

    ScreenSound = {
        Description = "Enable or disable sound.",
        Name = "Sound",
        YesNo = "config.common.yes_no.on_off",
    },

    ScreenMidiDevice = {
        Description = "Device to use for MIDI playback.\nOnly applies when using the generic MIDI driver.",
        Name = "MIDI Device",
    },

    ScreenPositionalAudio = {
        Description = "Whether or not to play certain sounds based on the position of the source.\nExamples are magic casting, eating/drinking, and damage.\n",
        Name = "Positional Audio",
    },

    ScreenMuteInBackground = {
        Name = "Mute In Background",
    },

    ScreenWindowMode = {
        Description = "Fullscreen mode.\n'Full screen' will use a hardware fullscreen mode.\n'Desktop fullscreen' will create a borderless window the same size as the screen.\n",
        Name = "Screen Mode",
        Choices = {
            DesktopFullscreen = "Desktop fullscreen",
            Fullscreen = "Full screen",
            Windowed = "Window mode",
        },
    },

    ScreenScreenResolution = {
        Name = "Screen Resolution",
        Description = "Screen resolution to use.\nThe available options may change depending on the graphics hardware you use.\n",
    },

    ScreenDisplayNumber = {
        Name = "Display Number",
    },

    ScreenUIScale = {
        Name = "UI Scaling",
    },

    ScreenTileScale = {
        Name = "Tile Scaling",
    },

    ScreenTileFilterMode = {
        Name = "Tile Filter Mode",
        Choices = {
            Linear = "Linear",
            Nearest = "Nearest",
        },
    },

    --
    -- Menu: Net
    --

    MenuNet = {
        Name = "Network Setting",
    },

    --
    -- Menu: Anime
    --

    MenuAnime = {
        Name = "Animation Setting",
    },

    AnimeAlertWait = {
        Name = "Alert Wait",
        Description = "Number of frames to wait if an important message is received.\nThese are: leveling up, leveling up a skill, or having a change in hunger status.\n",
    },

    AnimeAnimationWait = {
        Name = "Animation Wait",
        Description = "Number of frames to wait for animations.\nThis also acts as multiplier for the speed of auto-turn actions.\n",
        Formatter = "config.common.formatter.wait", -- TODO
    },

    AnimeScreenRefresh = {
        Name = "Screen Refresh",
        Description = "Number of frames to wait between updates to animations in the screen, like rain/lighting.\nThis does not affect key delay or animations that block input.\n",
        Formatter = "config.common.formatter.wait", -- TODO
    },

    AnimeBackgroundEffectWait = {
        Name = "Background Effect Wait",
    },

    AnimeObjectMovementSpeed = {
        Name = "Object Movement Speed",
    },

    --
    -- Menu: Input
    --

    MenuInput = {
        Name = "Input Setting",
    },

    --
    -- Menu: Keybindings
    --

    MenuKeybindings = {
        Name = "Keybindings",
    },

    --
    -- Menu: Message
    --

    MenuMessage = {
        Name = "Message & Log",
    },

    MessageFade = {
        Name = "Transparency",
    },

    --
    -- Menu: Language
    --

    MenuLanguage = {
        Name = "Language",
    },

    LanguageLanguage = {
        Name = "Language",
    },
}
