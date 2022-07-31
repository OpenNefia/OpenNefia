OpenNefia.Prototypes.Elona.StatusEffect.Elona = {
    Bleeding = {
        Apply = function(chara)
            return ("%s begin%s to bleed."):format(_.name(chara), _.s(chara))
        end,
        Heal = function(chara)
            return ("%s%s bleeding stops."):format(_.name(chara), _.his_owned(chara))
        end,
        Indicator = {
            ["0"] = "Bleeding",
            ["1"] = "Bleeding!",
            ["2"] = "Hemorrhage",
        },
    },
    Blindness = {
        Apply = function(chara)
            return ("%s %s blinded."):format(_.name(chara), _.is(chara))
        end,
        Heal = function(chara)
            return ("%s can see again."):format(_.name(chara))
        end,
        Indicator = "Blinded",
    },
    Confusion = {
        Apply = function(chara)
            return ("%s %s confused."):format(_.name(chara), _.is(chara))
        end,
        Heal = function(chara)
            return ("%s recover%s from confusion."):format(_.name(chara), _.s(chara))
        end,
        Indicator = "Confused",
    },
    Dimming = {
        Apply = function(chara)
            return ("%s %s dimmed."):format(_.name(chara), _.is(chara))
        end,
        Heal = function(chara)
            return ("%s regain%s %s senses."):format(_.name(chara), _.s(chara), _.his(chara))
        end,
        Indicator = {
            ["0"] = "Dim",
            ["1"] = "Muddled",
            ["2"] = "Unconscious",
        },
    },
    Drunk = {
        Apply = function(chara)
            return ("%s get%s drunk."):format(_.name(chara), _.s(chara))
        end,
        Heal = function(chara)
            return ("%s get%s sober."):format(_.name(chara), _.s(chara))
        end,
        Indicator = "Drunk",
    },
    Fear = {
        Apply = function(chara)
            return ("%s %s frightened."):format(_.name(chara), _.is(chara))
        end,
        Heal = function(chara)
            return ("%s shake%s off %s fear."):format(_.name(chara), _.s(chara), _.his(chara))
        end,
        Indicator = "Fear",
    },
    Insanity = {
        Apply = function(chara)
            return ("%s become%s insane."):format(_.name(chara), _.s(chara))
        end,
        Heal = function(chara)
            return ("%s come%s to %s again."):format(_.name(chara), _.s(chara), _.himself(chara))
        end,
        Indicator = {
            ["0"] = "Unsteady",
            ["1"] = "Insane",
            ["2"] = "Paranoia",
        },
    },
    Paralysis = {
        Apply = function(chara)
            return ("%s %s paralyzed."):format(_.name(chara), _.is(chara))
        end,
        Heal = function(chara)
            return ("%s recover%s from paralysis."):format(_.name(chara), _.s(chara))
        end,
        Indicator = "Paralyzed",
    },
    Poison = {
        Apply = function(chara)
            return ("%s %s poisoned."):format(_.name(chara), _.is(chara))
        end,
        Heal = function(chara)
            return ("%s recover%s from poison."):format(_.name(chara), _.s(chara))
        end,
        Indicator = {
            ["0"] = "Poisoned",
            ["1"] = "Poisoned Bad!",
        },
    },
    Sick = {
        Apply = function(chara)
            return ("%s get%s sick."):format(_.name(chara), _.s(chara))
        end,
        Heal = function(chara)
            return ("%s recover%s from %s illness."):format(_.name(chara), _.s(chara), _.his(chara))
        end,
        Indicator = {
            ["0"] = "Sick",
            ["1"] = "Very Sick",
        },
    },
    Sleep = {
        Apply = function(chara)
            return ("%s fall%s asleep."):format(_.name(chara), _.s(chara))
        end,
        Heal = function(chara)
            return ("%s awake%s from %s sleep."):format(_.name(chara), _.s(chara), _.his(chara))
        end,
        Indicator = {
            ["0"] = "Sleep",
            ["1"] = "Deep Sleep",
        },
    },
    Choking = {
        Indicator = "Choked",
    },
    Fury = {
        Indicator = {
            ["0"] = "Fury",
            ["1"] = "Berserk",
        },
    },
    Gravity = {
        Indicator = "Gravity",
    },
    Wet = {
        Indicator = "Wet",
    },
}
