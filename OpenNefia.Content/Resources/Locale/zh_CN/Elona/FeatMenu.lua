Elona.FeatMenu = {
    Window = {
        Title = "特征和体质",
    },
    Header = {
        Available = "◆ 可获得的特征",
        Gained = "◆ 特征和体质",
    },
    FeatMax = "最大",
    Topic = {
        Name = "特征名称",
        Detail = "特征效果",
    },
    FeatCount = function(featsRemaining)
        return ("还可以获得%s个特征"):format(featsRemaining)
    end,
    FeatType = {
        Feat = "特征",
        Race = "先天",
        Mutation = "变异",
        EtherDisease = "乙太病",
    },
}