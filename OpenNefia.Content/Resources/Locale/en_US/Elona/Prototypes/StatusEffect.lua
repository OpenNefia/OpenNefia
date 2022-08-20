OpenNefia.Prototypes.Elona.StatusEffect.Elona = {
    Bleeding = {
        Apply = function(chara)
            return ("%s begin%s to bleed."):format(_.name(chara), _.s(chara))
        end,
        Heal = function(chara)
            return ("%s bleeding stops."):format(_.possessive(chara))
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
        Indicator = {
            ["0"] = "Blinded",
        },
    },
    Confusion = {
        Apply = function(chara)
            return ("%s %s confused."):format(_.name(chara), _.is(chara))
        end,
        Heal = function(chara)
            return ("%s recover%s from confusion."):format(_.name(chara), _.s(chara))
        end,
        Indicator = {
            ["0"] = "Confused",
        },
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
        Indicator = {
            ["0"] = "Drunk",
            ["1"] = "Drunk",
        },
    },
    Fear = {
        Apply = function(chara)
            return ("%s %s frightened."):format(_.name(chara), _.is(chara))
        end,
        Heal = function(chara)
            return ("%s shake%s off %s fear."):format(_.name(chara), _.s(chara), _.his(chara))
        end,
        Indicator = {
            ["0"] = "Fear",
        },
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

        Dialog = {
            function(entity)
                return ("%s start%s to take %s clothes off."):format(_.name(entity), _.s(entity), _.his(entity))
            end,
            function(entity)
                return ("%s shout%s."):format(_.name(entity), _.s(entity))
            end,
            function(entity)
                return ("%s dance%s."):format(_.name(entity), _.s(entity))
            end,
            _.quote "Weeeeeee!",
            _.quote "Forgive me! Forgive me!",
            _.quote "P-P-Pika!",
            _.quote "Shhhhhh!",
            _.quote "So I have to kill.",
            _.quote "You snail!",
        },
    },
    Paralysis = {
        Apply = function(chara)
            return ("%s %s paralyzed."):format(_.name(chara), _.is(chara))
        end,
        Heal = function(chara)
            return ("%s recover%s from paralysis."):format(_.name(chara), _.s(chara))
        end,
        Indicator = {
            ["0"] = "Paralyzed",
        },
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
        Indicator = {
            ["0"] = "Choked",
        },
        Dialog = _.quote "Ughh...!",
    },
    Fury = {
        Indicator = {
            ["0"] = "Fury",
            ["1"] = "Berserk",
        },
    },
    Gravity = {
        Indicator = {
            ["0"] = "Gravity",
        },
    },
    Wet = {
        Indicator = {
            ["0"] = "Wet",
        },
    },
}
