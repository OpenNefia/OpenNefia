OpenNefia.Prototypes.Elona.ConfigMenuItem.Elona = {

    MenuDefault = {
        Name = "オプション",
    },

    --
    -- Menu: Game
    --

    MenuGame = {
        Name = "ゲーム設定",
    },

    --
    -- Menu: Screen
    --

    MenuScreen = {
        Name = "画面と音の設定",
    },

    ScreenMusic = {
        Name = "BGMの再生*",
        Description = "BGMを再生します。",
        YesNo = "config.common.yes_no.ari_nashi",
    },

    ScreenSound = {
        Name = "サウンドの再生*",
        Description = "SEを再生します。",
        YesNo = "config.common.yes_no.ari_nashi",
    },

    ScreenMidiDevice = {
        Name = "MIDIのデバイス",
    },

    ScreenPositionalAudio = {
        Description = "音が鳴った場所に応じてSEを再生します。\n例えば、画面左で鳴ったSEが左から聞こえるようになります。\n",
        Name = "ステレオサウンド",
    },

    ScreenWindowMode = {
        Description = "ウィンドウとフルスクリーンを切り替えます。\nフルスクリーン2は、スクリーンと同じサイズのウィンドウを生成し擬似的にフルスクリーンとします。\n",
        Name = "画面モード*",
        Choices = {
            Desktop = "フルスクリーン2",
            Exclusive = "フルスクリーン",
            Windowed = "ウィンドウ",
        },
    },

    ScreenScreenResolution = {
        Name = "画面の大きさ*",
    },

    --
    -- Menu: Net
    --

    MenuNet = {
        Name = "ネット機能の設定",
    },

    --
    -- Menu: Anime
    --

    MenuAnime = {
        Name = "アニメ設定",
    },

    --
    -- Menu: Input
    --

    MenuInput = {
        Name = "入力設定",
    },

    --
    -- Menu: Keybindings
    --

    MenuKeybindings = {
        Name = "キーの割り当て",
    },

    --
    -- Menu: Message
    --

    MenuMessage = {
        Name = "メッセージとログ",
    },

    --
    -- Menu: Language
    --

    MenuLanguage = {
        Name = "言語(Language)",
    },
}
