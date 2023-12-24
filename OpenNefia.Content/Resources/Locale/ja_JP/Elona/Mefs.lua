Elona.Mefs = {
    Fire = {
        IsBurnt = function(stepper)
            return ("%sは燃えた。"):format(_.name(stepper))
        end,
    },

    AcidGround = {
        Melts = function(stepper)
            return ("%sは酸に焼かれた。"):format(_.name(stepper))
        end,
    },

    Web = {
        Destroys = function(stepper)
            return ("%sは蜘蛛の巣を振り払った。"):format(_.name(stepper))
        end,
        Caught = function(stepper)
            return ("%sは蜘蛛の巣にひっかかった。"):format(_.name(stepper), _.is(stepper))
        end,
    },

    MistOfDarkness = {
        AttacksIllusion = function(attacker)
            return ("%sは霧の中の幻影を攻撃した。"):format(_.name(attacker))
        end,
    },
}
