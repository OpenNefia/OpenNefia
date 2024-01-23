Elona.Pregnancy = {
    Name = {
        ChildOf = function(entityName)
            return ("%sの子供"):format(entityName)
        end,
        AlienKid = function(basename)
            return ("%sの子供"):format(basename)
        end,
    },
    Apply = function(source, target)
        return ("%sは%sの口の中に何かを送り込んだ！"):format(_.name(source), _.name(target))
    end,
    Protected = "しかしすぐに吐き出した。",
    Impregnated = function(entity)
        return ("%sは寄生された。"):format(_.name(entity))
    end,

    PatsStomach = function(entity)
        return ("%sは不安げに腹を押さえた。"):format(_.name(entity))
    end,
    SomethingBreaksOut = function(entity)
        return ("何かが%sの腹を破り飛び出した！"):format(_.name(entity))
    end,

    AlienChildrenMelt = function(entity)
        return ("%sの体内のエイリアンは溶けた。"):format(_.name(entity))
    end,
    SpitsAlienChildren = function(entity)
        return ("%sは体内のエイリアンを吐き出した！"):format(_.name(entity))
    end,
}
