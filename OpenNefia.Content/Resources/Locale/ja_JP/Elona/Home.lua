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
    Rank = {
        Change = function(furnitureValue, heirloomValue, prevRank, newRank, newTitle, rankName)
            return ("家具(%s点) 家宝(%s点) ランク変動(%s %s位 → %s位 )《%s》"):format(
                furnitureValue,
                heirloomValue,
                rankName,
                prevRank,
                newRank,
                newTitle
            )
        end,
        Window = {
            Place = function(ordinal)
                return ("%s位."):format(ordinal)
            end,
            Star = "★",
            Title = "家の情報",
            Topic = {
                HeirloomRank = "家宝ランク",
                Value = "価値",
            },
            Type = {
                Base = "基本.",
                Deco = "家具.",
                Heir = "家宝.",
                Total = "総合.",
            },
        },
    },
}
