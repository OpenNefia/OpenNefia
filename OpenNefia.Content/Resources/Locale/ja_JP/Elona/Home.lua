Elona.Home = {
    Map = {
        Name = "わが家",
        Description = "あなたの家だ。",
    },
    ItemName = {
        Deed = function(home_name)
            return ("%sの"):format(home_name)
        end,
    },
    WelcomeHome = {
        _.quote "おかえり",
        _.quote "よう戻ったか",
        _.quote "無事で何よりです",
        _.quote "おかか♪",
        _.quote "待ってたよ",
        _.quote "おかえりなさい！",
    },
    Design = {
        Help = "マウスの左クリックでタイルの敷設、右クリックでタイルの取得、移動キーでスクリーン移動、決定キーでタイル一覧、キャンセルキーで終了。",
    },
}
