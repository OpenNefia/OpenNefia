Elona.Tone = {
    ChangeTone = {
        Prompt = "请告诉我要用什么语调。",
        IsSomewhatDifferent = function(entity)
            return ("%s的语调似乎有所变化。"):format(_.name(entity))
        end,

        Layer = {
            Hint = {
                Action = {
                    ShowHidden = "全部显示",
                    ChangeTone = "更改语调",
                },
            },
            DefaultTone = "默认语调",
            Title = "语调列表",
            ToneTitle = "标题",
            ModName = "MOD",
        },
    },
}