Elona.AI = {
    Ally = {
        SellsItems = function(_1, _2, _3)
            return ("%sは%s個のアイテムを売りさばき%sgoldを稼いだ。"):format(_.name(_1), _2, _3)
        end,
        VisitsTrainer = function(_1)
            return ("%sは訓練所に通い潜在能力を伸ばした！"):format(_.basename(_1))
        end,
    },
    CrushesWall = function(_1)
        return ("%sは壁を破壊した！"):format(_.name(_1))
    end,
    FireGiant = {
        _.quote "化け物め！",
        _.quote "くたばれっ",
        _.quote "退治してやるぅ！",
        _.quote "くらえー！",
    },
    MakesSnowman = function(_1, _2)
        return ("%sは%sを作った！"):format(_.name(_1), _2)
    end,
    Snail = { _.quote "なめくじだ！", _.quote "殺す！" },
    Snowball = {
        " *クスクス* ",
        _.quote "えいっ",
        _.quote "うりゃ",
        _.quote "くらえー！",
        _.quote "危ないっ！",
        _.quote "避けてー",
    },
    Swap = {
        Displaces = function(_1, _2)
            return ("%sは%sを押しのけた。"):format(_.name(_1), _.name(_2))
        end,
        Glares = function(_1, _2)
            return ("%sは%sを睨み付けた。"):format(_.name(_2), _.name(_1))
        end,
    },
}
