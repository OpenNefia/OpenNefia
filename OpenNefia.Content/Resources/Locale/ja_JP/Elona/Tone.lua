Elona.Tone = {
    ChangeTone = {
        Prompt = "どんな言葉を教えようか。",
        IsSomewhatDifferent = function(entity)
            return ("%sの口調が変わった気がする。"):format(_.name(entity))
        end,

        Layer = {
            Hint = {
                Action = {
                    ShowHidden = "全部表示",
                    ChangeTone = "口調の変更",
                },
            },
            DefaultTone = "デフォルトの口調",
            Title = "口調一覧",
            ToneTitle = "題名",
            ModName = "MOD",
        },
    },
}
