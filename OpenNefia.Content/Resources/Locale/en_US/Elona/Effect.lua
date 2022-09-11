Elona.Effect = {
    Common = {
        ItIsCursed = "これは呪われている！",
        CursedLaughter = function(target)
            return ("%sは悪魔が笑う声を聞いた。"):format(_.name(target))
        end,
        Resists = function(target)
            return ("%sは抵抗した。"):format(_.name(target))
        end,
    },

    Identify = {
        Fully = function(item)
            return ("The item is fully identified as %s."):format(_.name(item))
        end,
        Partially = function(item)
            return ("The item is half-identified as %s."):format(_.name(item))
        end,
        NeedMorePower = "You need higher identification to gain new knowledge.",
    },

    Uncurse = {
        Apply = {
            Normal = function(target)
                return ("%s equipment are surrounded by a white aura."):format(_.possessive(target))
            end,
            Blessed = function(target)
                return ("%s %s surrounded by a holy aura."):format(_.name(target), _.is(target))
            end,
        },

        Result = {
            Equipment = function(target)
                return ("The aura uncurses some of %s equipment."):format(_.his(target))
            end,
            Items = function(target)
                return ("The aura uncurses some %s stuff."):format(_.his(target))
            end,
            Resisted = "Several items resist the aura and remain cursed.",
        },
    },

    Curse = {
        Spell = function(source, target)
            return ("%s point%s %s and mutter%s a curse."):format(
                _.name(source),
                _.s(source),
                _.name(target),
                _.s(source)
            )
        end,
        Apply = function(target, item)
            return ("%s %s glow%s black."):format(_.possessive(target), _.name(item, true, 1), _.s(item))
        end,
        NoEffect = "Your prayer nullifies the curse.",
    },
}
