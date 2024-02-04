Elona.Buff = {
    Apply = {
        NoEffect = "然而，没有产生效果。",
        Repelled = "神圣面具防止了诅咒。",
        Resists = function(target)
            return ("%s抵抗了。"):format(_.name(target))
        end,
    },
    Ends = function(buff)
        return ("%s的效果消失了。"):format(_.name(buff))
    end,

    Types = {
        Speed = {
            Cursed = function(target)
                return ("%s的衰老速度变慢了。"):format(_.possessive(target))
            end,
        },
        Slow = {
            Blessed = function(target)
                return ("%s的衰老速度变快了。"):format(_.possessive(target))
            end,
        },
        DeathWord = {
            Breaks = "死亡词语破碎了。",
        },
    },
}