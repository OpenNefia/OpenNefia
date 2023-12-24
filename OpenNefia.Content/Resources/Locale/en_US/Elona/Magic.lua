-- TODO merge elsewhere
Elona.Magic = {
    FailToCast = {
        CreaturesAreSummoned = "Several creatures are summoned from a vortex of magic.",
        DimensionDoorOpens = function(chara)
            return ("A dimension door opens in front of %s."):format(_.name(chara))
        end,
        IsConfusedMore = function(chara)
            return ("%s %s confused more."):format(_.name(chara), _.is(chara))
        end,
        TooDifficult = "It's too difficult!",
        ManaIsAbsorbed = function(chara)
            return ("%s mana is absorbed."):format(_.possessive(chara))
        end,
    },

    ControlMagic = {
        PassesThrough = function(target)
            return ("The spell passes through %s."):format(_.name(target))
        end,
    },

    Message = {
        Generic = {
            Ally = function(entity)
                return ("It hits %s."):format(_.name(entity))
            end,
            Other = function(entity)
                return ("It hits %s and"):format(_.name(entity))
            end,
        },
        Arrow = {
            Ally = function(entity)
                return ("The arrow hits %s."):format(_.name(entity))
            end,
            Other = function(entity)
                return ("The arrow hits %s and"):format(_.name(entity))
            end,
        },
        Ball = {
            Ally = function(entity)
                return ("The ball hits %s."):format(_.name(entity))
            end,
            Other = function(entity)
                return ("The ball hits %s and"):format(_.name(entity))
            end,
        },
        Bolt = {
            Ally = function(entity)
                return ("The bolt hits %s."):format(_.name(entity))
            end,
            Other = function(entity)
                return ("The bolt hits %s and"):format(_.name(entity))
            end,
        },
        Breath = {
            Ally = function(entity)
                return ("The breath hits %s."):format(_.name(entity))
            end,
            Other = function(entity)
                return ("The breath hits %s and"):format(_.name(entity))
            end,

            Bellows = function(entity, breathName)
                return ("%s bellow%s %s from %s mouth."):format(_.name(entity), _.s(entity), breathName, _.his(entity))
            end,
            Named = function(breathName)
                return ("%s breath"):format(breathName)
            end,
            NoElement = "breath",
        },
        Summon = "Several monsters come out from a portal.",
        Mef = {
            AcidGround = "Acid puddles are generated.",
            EtherGround = "Ether mist spreads.",
            Fire = "Walls of fire come out from the ground.",
            MistOfDarkness = "The air is wrapped in a dense fog.",
            Web = "The ground is covered with thick webbing.",
        },
    },
}
