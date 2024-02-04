Elona.Home = {
    Map = {
        Name = "我的家",
        Description = "这是你的家。",
    },
    ItemName = {
        Deed = function(home_name)
            return ("%s的"):format(home_name)
        end,
    },
    WelcomeHome = {
        _.quote "欢迎回家",
        _.quote "又回来了啊",
        _.quote "平安归来真是太好了",
        _.quote "欢迎♪",
        _.quote "一直在等你",
        _.quote "欢迎回来！",
    },
    Design = {
        Help = "鼠标左键放置瓷砖，鼠标右键取出瓷砖，移动键移动屏幕，确认键查看瓷砖列表，取消键退出。",
    },
    Rank = {
        Change = function(furnitureValue, heirloomValue, prevRank, newRank, newTitle, rankName)
            return ("家具（%s点） 家宝（%s点） 排名变动（%s %s位 → %s位 ）《%s》"):format(
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
            Title = "家的信息",
            Topic = {
                HeirloomRank = "家宝排名",
                Value = "价值",
            },
            Type = {
                Base = "基本.",
                Deco = "家具.",
                Heir = "家宝.",
                Total = "综合.",
            },
        },
    },
}