Elona.Magic = {
    FailToCast = {
        CreaturesAreSummoned = "魔力激流召唤了什么东西！",
        DimensionDoorOpens = function(chara)
            return ("%s被奇异的力量扭曲了！"):format(_.name(chara))
        end,
        IsConfusedMore = function(chara)
            return ("%s变得更加混乱。"):format(_.name(chara))
        end,
        TooDifficult = "太难了！",
        ManaIsAbsorbed = function(chara)
            return ("%s的魔力被吸取了！"):format(_.name(chara))
        end,
    },

    ControlMagic = {
        PassesThrough = function(source, target)
            return ("%s避开了被卷入！"):format(_.name(target))
        end,
    },

    Message = {
        Generic = {
            Ally = function(source, entity)
                return ("%s命中了。"):format(_.name(entity))
            end,
            Other = function(source, entity)
                return ("%s命中了"):format(_.name(entity))
            end,
        },
        Arrow = {
            Ally = function(source, entity)
                return ("箭矢命中了%s。"):format(_.name(entity))
            end,
            Other = function(source, entity)
                return ("箭矢命中了%s"):format(_.name(entity))
            end,
        },
        Ball = {
            Ally = function(source, entity)
                return ("球形物命中了%s。"):format(_.name(entity))
            end,
            Other = function(source, entity)
                return ("球形物命中了%s"):format(_.name(entity))
            end,
        },
        Bolt = {
            Ally = function(source, entity)
                return ("霰弹命中了%s。"):format(_.name(entity))
            end,
            Other = function(source, entity)
                return ("霰弹命中了%s"):format(_.name(entity))
            end,
        },
        Breath = {
            Ally = function(source, entity)
                return ("气息命中了%s。"):format(_.name(entity))
            end,
            Other = function(source, entity)
                return ("气息命中了%s"):format(_.name(entity))
            end,

            Bellows = function(entity, breathName)
                return ("%s喷出了%s气息。"):format(_.name(entity), breathName)
            end,
            Named = function(breathName)
                return ("%s的"):format(breathName)
            end,
            NoElement = "",
        },
        Touch = {
            Ally = function(source, entity, elementStyle, meleeStyle)
                local style = _.loc("Elona.Damage.UnarmedText." .. meleeStyle .. ".Style")
                local verb = _.loc("Elona.Damage.UnarmedText." .. meleeStyle .. ".VerbPassive")
                return ("%s使用%s%s以%s的方式击中了%s"):format(_.name(source), elementStyle, style, verb, _.name(entity))
            end,
            Other = function(source, entity, elementStyle, meleeStyle)
                local style = _.loc("Elona.Damage.UnarmedText." .. meleeStyle .. ".Style")
                local verb = _.loc("Elona.Damage.UnarmedText." .. meleeStyle .. ".VerbActive")
                return ("%s%s使用%s%s以%s的方式击中了%s"):format(_.name(source), _.name(entity), elementStyle, style, verb)
            end,
        },
        Summon = "使用魔法召唤了怪物。",
        Mef = {
            AcidGround = "发生了酸水积淀。",
            EtherGround = "发生了虚灵之雾。",
            Fire = "发生了火柱。",
            MistOfDarkness = "周围弥漫着浓雾。",
            Web = "周围布满了蜘蛛网。",
        },
    },
}