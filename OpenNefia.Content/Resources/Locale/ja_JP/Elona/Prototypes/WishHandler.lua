OpenNefia.Prototypes.Elona.WishHandler.Elona = {
    Alias = {
        Keyword = { "通り名", "異名" },

        Prompt = "新しい異名は？",
        Impossible = "だめよ。",
        NewAlias = function(_1)
            return ("あなたの新しい異名は「%s」。満足したかしら？"):format(_1)
        end,
        NoChange = "あら、そのままでいいの？",
    },
    Ally = {
        Keyword = "仲間",
    },
    Death = {
        Keyword = "死",
        Result = "それがお望みなら…",
    },
    Fame = {
        Keyword = "名声",
    },
    GodInside = {
        Keyword = "中の神",
        Result = "中の神も大変…あ…中の神なんているわけないじゃない！…ねえ、聞かなかったことにしてね。",
    },
    Gold = {
        Keyword = { "金", "金貨", "富", "財産" },
        Result = "金貨が降ってきた！",
    },
    ManInside = {
        Keyword = "中の人",
        Result = "中の人も大変ね。",
    },
    Platinum = {
        Keyword = { "プラチナ", "プラチナ硬貨" },
        Result = "プラチナ硬貨が降ってきた！",
    },
    Redemption = {
        Keyword = "贖罪",
        NotASinner = "…罪なんて犯してないじゃない。",
        Result = "あら…都合のいいことを言うのね。",
    },
    Sex = {
        Keyword = { "性転換", "性", "異性" },
        Result = function(wisher, newGender)
            local genderName = _.loc("Elona.Gender.Names." .. newGender .. ".Normal")
            return ("%sは%sになった！ …もう後戻りはできないわよ。"):format(
                _.name(wisher),
                genderName
            )
        end,
    },
    SmallMedal = {
        Keyword = { "メダル", "小さなメダル", "ちいさなメダル" },
        Result = "小さなメダルが降ってきた！",
    },
    Youth = {
        Keyword = { "若さ", "若返り", "年", "美貌" },
        Result = "ふぅん…そんな願いでいいんだ。",
    },
}
