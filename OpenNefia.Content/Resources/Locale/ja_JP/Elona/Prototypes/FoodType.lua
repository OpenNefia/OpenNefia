OpenNefia.Prototypes.Elona.FoodType.Elona = {
    Meat = {
        DefaultOrigin = "動物",
        UncookedMessage = "生肉だ…",
        Names = {
            ["1"] = function(_1)
                return ("グロテスクな%sの肉"):format(_1)
            end,
            ["2"] = function(_1)
                return ("焼け焦げた%sの肉"):format(_1)
            end,
            ["3"] = function(_1)
                return ("%sのこんがり肉"):format(_1)
            end,
            ["4"] = function(_1)
                return ("%s肉のオードブル"):format(_1)
            end,
            ["5"] = function(_1)
                return ("%sのピリ辛炒め"):format(_1)
            end,
            ["6"] = function(_1)
                return ("%sコロッケ"):format(_1)
            end,
            ["7"] = function(_1)
                return ("%sのハンバーグ"):format(_1)
            end,
            ["8"] = function(_1)
                return ("%s肉の大葉焼き"):format(_1)
            end,
            ["9"] = function(_1)
                return ("%sステーキ"):format(_1)
            end,
        },
    },
    Vegetable = {
        DefaultOrigin = "野菜",
        Names = {
            ["1"] = function(_1)
                return ("生ごみ同然の%s"):format(_1)
            end,
            ["2"] = function(_1)
                return ("悪臭を放つ%s"):format(_1)
            end,
            ["3"] = function(_1)
                return ("%sのサラダ"):format(_1)
            end,
            ["4"] = function(_1)
                return ("%sの炒め物"):format(_1)
            end,
            ["5"] = function(_1)
                return ("%s風味の肉じゃが"):format(_1)
            end,
            ["6"] = function(_1)
                return ("%sの天ぷら"):format(_1)
            end,
            ["7"] = function(_1)
                return ("%sの煮込み"):format(_1)
            end,
            ["8"] = function(_1)
                return ("%sシチュー"):format(_1)
            end,
            ["9"] = function(_1)
                return ("%s風カレー"):format(_1)
            end,
        },
    },
    Fruit = {
        DefaultOrigin = "果物",
        Names = {

            ["1"] = function(_1)
                return ("食べてはならない%s"):format(_1)
            end,
            ["2"] = function(_1)
                return ("べっちょりした%s"):format(_1)
            end,
            ["3"] = function(_1)
                return ("%sのフルーツサラダ"):format(_1)
            end,
            ["4"] = function(_1)
                return ("%sのプリン"):format(_1)
            end,
            ["5"] = function(_1)
                return ("%sシャーベット"):format(_1)
            end,
            ["6"] = function(_1)
                return ("%sシェイク"):format(_1)
            end,
            ["7"] = function(_1)
                return ("%sクレープ"):format(_1)
            end,
            ["8"] = function(_1)
                return ("%sフルーツケーキ"):format(_1)
            end,
            ["9"] = function(_1)
                return ("%sパフェ"):format(_1)
            end,
        },
    },
    Sweet = {
        DefaultOrigin = "お菓子",
        Names = {
            ["1"] = function(_1)
                return ("原型を留めない%s"):format(_1)
            end,
            ["2"] = function(_1)
                return ("まずそうな%s"):format(_1)
            end,
            ["3"] = function(_1)
                return ("%sクッキー"):format(_1)
            end,
            ["4"] = function(_1)
                return ("%sのゼリー"):format(_1)
            end,
            ["5"] = function(_1)
                return ("%sパイ"):format(_1)
            end,
            ["6"] = function(_1)
                return ("%sまんじゅう"):format(_1)
            end,
            ["7"] = function(_1)
                return ("%s風味のシュークリーム"):format(_1)
            end,
            ["8"] = function(_1)
                return ("%sのケーキ"):format(_1)
            end,
            ["9"] = function(_1)
                return ("%s風ザッハトルテ"):format(_1)
            end,
        },
    },
    Pasta = {
        DefaultOrigin = "麺",
        UncookedMessage = "生で食べるものじゃないな…",
        Names = {
            ["1"] = function(_1)
                return ("禁断の%s"):format(_1)
            end,
            ["2"] = function(_1)
                return ("のびてふにゃった%s"):format(_1)
            end,
            ["3"] = "サラダパスタ",
            ["4"] = "うどん",
            ["5"] = "冷やし蕎麦",
            ["6"] = "ペペロンチーノ",
            ["7"] = "カルボナーラ",
            ["8"] = "ラーメン",
            ["9"] = "ミートスパゲティ",
        },
    },
    Fish = {
        DefaultOrigin = "魚",
        Names = {
            ["1"] = function(_1)
                return ("%sの残骸"):format(_1)
            end,
            ["2"] = function(_1)
                return ("骨だけ残った%s"):format(_1)
            end,
            ["3"] = function(_1)
                return ("%sのフライ"):format(_1)
            end,
            ["4"] = function(_1)
                return ("%sの煮込み"):format(_1)
            end,
            ["5"] = function(_1)
                return ("%sスープ"):format(_1)
            end,
            ["6"] = function(_1)
                return ("%sの天ぷら"):format(_1)
            end,
            ["7"] = function(_1)
                return ("%sソーセージ"):format(_1)
            end,
            ["8"] = function(_1)
                return ("%sの刺身"):format(_1)
            end,
            ["9"] = function(_1)
                return ("%sの活け作り"):format(_1)
            end,
        },
    },
    Bread = {
        DefaultOrigin = "パン",
        UncookedQuality = "粉の味がする…",
        Names = {
            ["1"] = function(_1)
                return ("恐怖の%s"):format(_1)
            end,
            ["2"] = function(_1)
                return ("ガチガチの%s"):format(_1)
            end,
            ["3"] = "くるみパン",
            ["4"] = "アップルパイ",
            ["5"] = "サンドイッチ",
            ["6"] = "クロワッサン",
            ["7"] = "コロッケパン",
            ["8"] = "カレーパン",
            ["9"] = "メロンパン",
        },
    },
    Egg = {
        DefaultOrigin = "鳥",
        Names = {
            ["1"] = function(_1)
                return ("グロテスクな%sの卵"):format(_1)
            end,
            ["2"] = function(_1)
                return ("焦げた%sの卵"):format(_1)
            end,
            ["3"] = function(_1)
                return ("%sの卵の目玉焼き"):format(_1)
            end,
            ["4"] = function(_1)
                return ("%s風味のキッシュ"):format(_1)
            end,
            ["5"] = function(_1)
                return ("半熟%s"):format(_1)
            end,
            ["6"] = function(_1)
                return ("%sの卵入りスープ"):format(_1)
            end,
            ["7"] = function(_1)
                return ("熟成%sチーズ"):format(_1)
            end,
            ["8"] = function(_1)
                return ("%sのレアチーズケーキ"):format(_1)
            end,
            ["9"] = function(_1)
                return ("%s風オムライス"):format(_1)
            end,
        },
    },
}
