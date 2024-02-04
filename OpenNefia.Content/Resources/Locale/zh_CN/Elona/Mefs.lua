Elona.Mefs = {
    Fire = {
        IsBurnt = function(stepper)
            return ("%s已经燃烧。"):format(_.name(stepper))
        end,
    },

    AcidGround = {
        Melts = function(stepper)
            return ("%s被酸溶解。"):format(_.name(stepper))
        end,
    },

    Web = {
        Destroys = function(stepper)
            return ("%s甩开了蜘蛛网。"):format(_.name(stepper))
        end,
        Caught = function(stepper)
            return ("%s被蜘蛛网绊住了。"):format(_.name(stepper), _.is(stepper))
        end,
    },

    MistOfDarkness = {
        AttacksIllusion = function(attacker)
            return ("%s攻击了迷雾中的幻影。"):format(_.name(attacker))
        end,
    },
}