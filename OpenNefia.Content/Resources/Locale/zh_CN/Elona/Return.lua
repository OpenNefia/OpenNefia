Elona.Return = {
    Prompt = "要返回哪里？",
    LevelCounter = function(level)
        return ("%s楼"):format(level)
    end,
    NoLocations = "在这个大陆上没有可以返回的地方。",
    Begin = "周围的空气开始震动。",
    Cancel = function(source)
        return ("%s取消了返回。"):format(_.sore_wa(source))
    end,
    Prevented = "神秘的力量阻止了返回。",

    Result = {
        AllyPrevents = "您正在携带无法返回的伙伴。",
        Overburdened = "你听到了声音：“抱歉，你负重超过了。”",
        CargoOverburdened = "货车过重，无法返回。",
        CommitCrime = "你违反了法律。",
        DimensionalDoorOpens = "你打开了次元之门。",
        SentToJail = "时光管理者的心血来潮扭曲了时空！",
    },
}