Elona.Pregnancy = {
    Name = {
        ChildOf = function(entityName)
            return ("child of %s"):format(entityName)
        end,
        AlienKid = function(basename)
            return ("%s kid"):format(basename)
        end,
    },
    ButPukesOut = function(entity)
        return ("But %s puke%s it out quickly."):format(_.he(entity), _.s(entity))
    end,
    Impregnated = function(entity)
        return ("%s get%s pregnant."):format(_.name(entity), _.s(entity))
    end,

    PatsStomach = function(entity)
        return ("%s pat%s %s stomach uneasily."):format(_.name(entity), _.s(entity), _.his(entity))
    end,
    SomethingBreaksOut = function(entity)
        return ("Something splits %s body and breaks out!"):format(_.possessive(entity))
    end,

    AlienChildrenMelt = function(entity)
        return ("%s alien children melt in %s stomach."):format(_.possessive(entity), _.his(entity))
    end,
    SpitsAlienChildren = function(entity)
        return ("%s spit%s alien children from %s body!"):format(_.name(entity), _.s(entity), _.his(entity))
    end,
}
