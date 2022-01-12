Elona.FeatMenu = {
    Window = {
        Title = "特徴と体質",
    },
    Header = {
        Available = "◆ 取得できるフィート",
        Gained = "◆ 特徴と体質",
    },
    FeatMax = "MAX",
    Topic = {
        Name = "特徴の名称",
        Detail = "特徴の効果",
    },
    FeatCount = function(featsRemaining)
        return ("残り %s個のフィートを取得できる"):format(featsRemaining)
    end,
    FeatType = {
        Feat = "フィート",
        Race = "先天",
        Mutation = "変異",
        EtherDisease = "ｴｰﾃﾙ病",
    },
}
