Elona.Mefs = {
    Fire = {
        IsBurnt = function(stepper)
            return ("%s %s burnt."):format(_.name(stepper), _.is(stepper))
        end,
    },

    AcidGround = {
        Melts = function(stepper)
            return ("%s melt%s."):format(_.name(stepper), _.s(stepper))
        end,
    },

    Web = {
        Destroys = function(stepper)
            return ("%s destroy%s the cobweb."):format(_.name(stepper), _.s(stepper))
        end,
        Caught = function(stepper)
            return ("%s %s caught in a cobweb."):format(_.name(stepper), _.is(stepper))
        end,
    },

    MistOfDarkness = {
        AttacksIllusion = function(attacker)
            return ("%s attack%s an illusion in the mist."):format(_.name(attacker), _.s(attacker))
        end,
    },
}
