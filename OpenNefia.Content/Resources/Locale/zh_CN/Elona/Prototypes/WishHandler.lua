OpenNefia.Prototypes.Elona.WishHandler.Elona = {
    Alias = {
        Keyword = { "通名", "別名" },

        Prompt = "请输入新的別名：",
        Impossible = "不行哦。",
        NewAlias = function(_1)
            return ("你的新別名是“%s”。感到满意了吗？"):format(_1)
        end,
        NoChange = "哎呀，就这样吗？",
    },
    Ally = {
        Keyword = "伙伴",
    },
    Death = {
        Keyword = "死亡",
        Result = "如果你真的这么想的话……",
    },
    Fame = {
        Keyword = "名声",
    },
    GodInside = {
        Keyword = "内部神灵",
        Result = "连内部神灵也不行…啊…内部神灵什么的根本不存在！…喂，就当我没问过。",
    },
    Gold = {
        Keyword = { "金币", "富贵", "财产" },
        Result = "金币掉了下来！",
    },
    ManInside = {
        Keyword = "内部人员",
        Result = "连内部人员都不好受呢。",
    },
    Platinum = {
        Keyword = { "白金", "白金币" },
        Result = "白金币掉了下来！",
    },
    Redemption = {
        Keyword = "救赎",
        NotASinner = "…我可没犯什么罪。",
        Result = "啊…说得真合适。",
    },
    Sex = {
        Keyword = { "性转换", "性别", "异性" },
        Result = function(wisher, newGender)
            local genderName = _.loc("Elona.Gender.Names." .. newGender .. ".Normal")
            return ("%s成为了%s！ …已经无法回头了。"):format(
                _.name(wisher),
                genderName
            )
        end,
    },
    SmallMedal = {
        Keyword = { "勋章", "小勋章" },
        Result = "小勋章掉了下来！",
    },
    Youth = {
        Keyword = { "年轻", "返老还童", "年龄", "容貌" },
        Result = "哦～这样的愿望也行。",
    },
}