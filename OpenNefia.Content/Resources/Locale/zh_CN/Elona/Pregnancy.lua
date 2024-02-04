Elona.Pregnancy = {
    Name = {
        ChildOf = function(entityName)
            return ("%s的孩子"):format(entityName)
        end,
        AlienKid = function(basename)
            return ("%s的孩子"):format(basename)
        end,
    },
    Apply = function(source, target)
        return ("%s向%s的口中送了些东西！"):format(_.name(source), _.name(target))
    end,
    Protected = "但很快就吐了出来。",
    Impregnated = function(entity)
        return ("%s被寄生了。"):format(_.name(entity))
    end,

    PatsStomach = function(entity)
        return ("%s焦虑地按住自己的肚子。"):format(_.name(entity))
    end,
    SomethingBreaksOut = function(entity)
        return ("有什么东西从%s的肚子里冲出来了！"):format(_.name(entity))
    end,

    AlienChildrenMelt = function(entity)
        return ("%s体内的外星人融化了。"):format(_.name(entity))
    end,
    SpitsAlienChildren = function(entity)
        return ("%s吐出了体内的外星人！"):format(_.name(entity))
    end,
}