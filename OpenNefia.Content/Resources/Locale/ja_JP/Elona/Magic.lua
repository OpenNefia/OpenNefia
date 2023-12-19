-- TODO merge elsewhere
Elona.Magic = {
    FailToCast = {
        CreaturesAreSummoned = "魔力の渦が何かを召喚した！",
        DimensionDoorOpens = function(chara)
            return ("%sは奇妙な力に捻じ曲げられた！"):format(_.name(chara))
        end,
        IsConfusedMore = function(chara)
            return ("%sは余計に混乱した。"):format(_.name(chara))
        end,
        TooDifficult = "難解だ！",
        ManaIsAbsorbed = function(chara)
            return ("%sはマナを吸い取られた！"):format(_.name(chara))
        end,
    },

    ControlMagic = {
        PassesThrough = function(target)
            return ("%sは巻き込みを免れた。"):format(_.name(target))
        end,
    },

    Message = {
        Generic = {
            Ally = function(entity)
                return ("%sに命中した。"):format(_.name(entity))
            end,
            Other = function(entity)
                return ("%sに命中し"):format(_.name(entity))
            end,
        },
        Arrow = {
            Ally = function(entity)
                return ("矢が%sに命中した。"):format(_.name(entity))
            end,
            Other = function(entity)
                return ("矢は%sに命中し"):format(_.name(entity))
            end,
        },
        Ball = {
            Ally = function(entity)
                return ("ボールが%sに命中した。"):format(_.name(entity))
            end,
            Other = function(entity)
                return ("ボールは%sに命中し"):format(_.name(entity))
            end,
        },
        Bolt = {
            Ally = function(entity)
                return ("ボルトが%sに命中した。"):format(_.name(entity))
            end,
            Other = function(entity)
                return ("ボルトは%sに命中し"):format(_.name(entity))
            end,
        },
        Breath = {
            Ally = function(entity)
                return ("ブレスは%sに命中した。"):format(_.name(entity))
            end,
            Other = function(entity)
                return ("ブレスは%sに命中し"):format(_.name(entity))
            end,

            Bellows = function(entity, breathName)
                return ("%sは%sブレスを吐いた。"):format(_.name(entity), breathName)
            end,
            Named = function(breathName)
                return ("%sの"):format(breathName)
            end,
            NoElement = "",
        },
        Summon = "魔法でモンスターが召喚された。",
    },
}
