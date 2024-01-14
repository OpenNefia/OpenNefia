Elona.Buff = {
    Apply = {
        NoEffect = "But it produces no effect.",
        Repelled = "The holy veil repels the hex.",
        Resists = function(target)
            return ("%s resist%s the hex."):format(_.name(target), _.s(target))
        end,
    },
    Ends = function(buff)
        return ("The effect of %s ends."):format(_.name(buff))
    end,

    Types = {
        Speed = {
            Cursed = function(target)
                return ("%sの老化は速くなった。"):format(_.name(target))
            end,
        },
        Slow = {
            Blessed = function(target)
                return ("%sの老化は遅くなった。"):format(_.name(target))
            end,
        },
        DeathWord = {
            Breaks = "死の宣告は無効になった。",
        },
    },
}
