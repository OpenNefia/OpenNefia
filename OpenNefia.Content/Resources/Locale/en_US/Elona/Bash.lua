Elona.Bash = {
    Prompt = "Which direction do you want to bash? ",
    Air = function(basher)
        return ("%s bash%s up the air."):format(_.name(basher), _.s(basher, true))
    end,
    Execute = function(basher, target)
        return ("%s bash%s up %s."):format(_.name(basher), _.s(basher, true), _.name(target))
    end,
    DisturbsSleep = function(basher, target)
        return ("%s disturb%s %s sleep."):format(_.name(basher), _.s(basher), _.his(target))
    end,
    Choking = {
        Dialog = _.quote "You saved me!",
        Execute = function(basher, target)
            return ("%s bash%s up %s at full power."):format(_.name(basher), _.s(basher, true), _.name(target))
        end,
        Spits = function(target)
            return ("%s spit%s mochi."):format(_.name(target), _.s(target))
        end,
    },
}
