OpenNefia.Prototypes.Elona.ConfigMenuItem.Elona = {
    MenuDefault = {
        Name = "选项",
    },

    --
    -- 菜单：游戏
    --

    MenuGame = {
        Name = "游戏设定",
    },

    --
    -- 菜单：画面
    --

    MenuScreen = {
        Name = "画面和音效设定",
    },

    ScreenMusic = {
        Name = "背景音乐",
        Description = "播放背景音乐",
        YesNo = "是/否",
    },

    ScreenSound = {
        Name = "音效",
        Description = "播放音效",
        YesNo = "是/否",
    },

    ScreenMidiDevice = {
        Name = "MIDI 设备",
    },

    ScreenPositionalAudio = {
        Description = "根据声音的来源播放音效。\n例如，在屏幕左边播放的音效将从左侧听到。\n",
        Name = "立体声音效",
    },

    ScreenMuteInBackground = {
        Name = "后台静音",
    },

    ScreenWindowMode = {
        Description = "切换窗口模式和全屏模式。\n\'Fullscreen 2\' 会创建一个与屏幕大小相同的窗口，并模拟全屏模式。\n",
        Name = "屏幕模式",
        Choices = {
            DesktopFullscreen = "Fullscreen 2",
            Fullscreen = "Fullscreen",
            Windowed = "窗口",
        },
    },

    ScreenScreenResolution = {
        Name = "屏幕分辨率",
    },

    ScreenDisplayNumber = {
        Name = "显示器编号",
    },

    ScreenUIScale = {
        Name = "界面缩放",
    },

    ScreenTileScale = {
        Name = "图块缩放",
    },

    ScreenTileFilterMode = {
        Name = "图块过滤模式",
        Choices = {
            Linear = "线性",
            Nearest = "最近",
        },
    },

    ScreenFontHeightScale = {
        Name = "字体高度缩放",
    },

    --
    -- 菜单：网络
    --

    MenuNet = {
        Name = "网络功能设定",
    },

    --
    -- 菜单：动画
    --

    MenuAnime = {
        Name = "动画设定",
    },

    AnimeAlertWait = {
        Name = "警报等待时间",
        Description = "重要信息显示的等待时间",
    },

    AnimeAnimationWait = {
        Name = "动画等待时间",
        Description = "动画的播放时间",
        Formatter = "config.common.formatter.wait", -- TODO
    },

    AnimeScreenRefresh = {
        Name = "屏幕刷新频率",
        Formatter = "config.common.formatter.wait", -- TODO
    },

    AnimeBackgroundEffectWait = {
        Name = "背景效果等待时间",
    },

    AnimeObjectMovementSpeed = {
        Name = "物体移动速度",
    },

    --
    -- 菜单：输入
    --

    MenuInput = {
        Name = "输入设置",
    },

    --
    -- 菜单：按键绑定
    --

    MenuKeybindings = {
        Name = "按键设置",
    },

    --
    -- 菜单：信息
    --

    MenuMessage = {
        Name = "消息和日志",
    },

    MessageFade = {
        Name = "透明度",
    },


    MenuLanguage = {
        Name = "语言",
    },

    LanguageLanguage = {
        Name = "语言",
    },
}