Elona.TargetText = {
    DangerLevel = {
        ["0"] = "目隠しして座っていても勝てる。",
        ["1"] = "目隠ししていても勝てそうだ。",
        ["2"] = "負ける気はしない。",
        ["3"] = "たぶん勝てそうだ。",
        ["4"] = "勝てない相手ではない。",
        ["5"] = "相手はかなり強そうだ。",
        ["6"] = "少なくとも、あなたの倍は強そうだ。",
        ["7"] = "奇跡が起きなければ殺されるだろう。",
        ["8"] = "確実に殺されるだろう。",
        ["9"] = "絶対に勝てない相手だ。",
        ["10"] = "相手が巨人だとすれば、あなたは蟻のフン以下だ。",
    },
    ItemOnCell = {
        And = "と",

        MoreThanThree = function(itemCount)
            return ("ここには%s種類のアイテムがある。"):format(itemCount)
        end,

        Item = function(itemNames)
            return ("%sが落ちている。"):format(itemNames)
        end,
        Construct = function(itemNames)
            return ("%sが設置されている。"):format(itemNames)
        end,
        NotOwned = function(itemNames)
            return ("%sがある。"):format(itemNames)
        end,
    },
}
