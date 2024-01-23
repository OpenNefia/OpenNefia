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
        PassesThrough = function(source, target)
            return ("%sは巻き込みを免れた。"):format(_.name(target))
        end,
    },

    Message = {
        Generic = {
            Ally = function(source, entity)
                return ("%sに命中した。"):format(_.name(entity))
            end,
            Other = function(source, entity)
                return ("%sに命中し"):format(_.name(entity))
            end,
        },
        Arrow = {
            Ally = function(source, entity)
                return ("矢が%sに命中した。"):format(_.name(entity))
            end,
            Other = function(source, entity)
                return ("矢は%sに命中し"):format(_.name(entity))
            end,
        },
        Ball = {
            Ally = function(source, entity)
                return ("ボールが%sに命中した。"):format(_.name(entity))
            end,
            Other = function(source, entity)
                return ("ボールは%sに命中し"):format(_.name(entity))
            end,
        },
        Bolt = {
            Ally = function(source, entity)
                return ("ボルトが%sに命中した。"):format(_.name(entity))
            end,
            Other = function(source, entity)
                return ("ボルトは%sに命中し"):format(_.name(entity))
            end,
        },
        Breath = {
            Ally = function(source, entity)
                return ("ブレスは%sに命中した。"):format(_.name(entity))
            end,
            Other = function(source, entity)
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
        Touch = {
            Ally = function(source, entity, elementStyle, meleeStyle)
                local style = _.loc("Elona.Damage.UnarmedText." .. meleeStyle .. ".Style")
                local verb = _.loc("Elona.Damage.UnarmedText." .. meleeStyle .. ".VerbPassive")
                return ("%sは%sに%s%sで%s"):format(_.name(source), _.name(entity), elementStyle, style, verb)
            end,
            Other = function(source, entity, elementStyle, meleeStyle)
                local style = _.loc("Elona.Damage.UnarmedText." .. meleeStyle .. ".Style")
                local verb = _.loc("Elona.Damage.UnarmedText." .. meleeStyle .. ".VerbActive")
                return ("%s%sを%s%sで%s"):format(_.name(source), _.name(entity), elementStyle, style, verb)
            end,
        },
        Summon = "魔法でモンスターが召喚された。",
        Mef = {
            AcidGround = "酸の水溜りが発生した。",
            EtherGround = "エーテルの霧が発生した。",
            Fire = "火柱が発生した。",
            MistOfDarkness = "辺りを濃い霧が覆った。",
            Web = "蜘蛛の巣が辺りを覆った。",
        },
    },
}
