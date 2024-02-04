Elona.TargetText = {
    DangerLevel = {
        ["0"] = "无论你闭着眼睛坐着，都能赢。",
        ["1"] = "闭着眼睛也能赢。",
        ["2"] = "我觉得不会输。",
        ["3"] = "大概能赢。",
        ["4"] = "不是对手，但也能打赢。",
        ["5"] = "对手看起来很强。",
        ["6"] = "至少是你的两倍强。",
        ["7"] = "如果没有奇迹的话，你会被杀掉。",
        ["8"] = "肯定会被杀掉。",
        ["9"] = "绝对打不过对手。",
        ["10"] = "如果对手是巨人，你连蝼蚁都不如。",
    },
    ItemOnCell = {
        And = "和",

        MoreThanThree = function(itemCount)
            return ("这里有%s种物品。"):format(itemCount)
        end,

        Item = function(itemNames)
            return ("掉落了%s。"):format(itemNames)
        end,
        Construct = function(itemNames)
            return ("摆放了%s。"):format(itemNames)
        end,
        NotOwned = function(itemNames)
            return ("有%s。"):format(itemNames)
        end,
    },
}