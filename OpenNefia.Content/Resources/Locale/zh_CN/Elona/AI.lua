Elona.AI = {
    Ally = {
        SellsItems = function(_1, _2, _3)
            return ("%s卖出了%s个物品，赚取了%s金币。"):format(_.name(_1), _2, _3)
        end,
        VisitsTrainer = function(_1)
            return ("%s去了训练所，提升了潜能！"):format(_.basename(_1))
        end,
    },
    CrushesWall = function(_1)
        return ("%s破坏了墙壁！"):format(_.name(_1))
    end,
    FireGiant = {
        _.quote "你这个怪物！",
        _.quote "滚开！",
        _.quote "我要消灭你！",
        _.quote "吃我一拳！",
    },
    MakesSnowman = function(_1, _2)
        return ("%s制造了%s！"):format(_.name(_1), _2)
    end,
    Snail = { _.quote "这是蜗牛！", _.quote "杀了你！" },
    Snowball = {
        " *嘻嘻* ",
        _.quote "嘿！",
        _.quote "噢！",
        _.quote "吃我一击！",
        _.quote "危险！",
        _.quote "躲开！",
    },
    Swap = {
        Displaces = function(_1, _2)
            return ("%s把%s推开。"):format(_.name(_1), _.name(_2))
        end,
        Glares = function(_1, _2)
            return ("%s瞪了%s一眼。"):format(_.name(_2), _.name(_1))
        end,
    },
}