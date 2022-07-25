Elona.Activity = {
    DefaultVerb = "current action",
    Cancel = {
        Normal = function(actor, activity)
            return ("%s stop%s %s."):format(_.name(actor), _.s(actor), _.name(activity))
        end,
        Item = function(actor)
            return ("%s cancel%s %s action."):format(_.name(actor), _.s(actor), _.his(actor))
        end,
        Prompt = function(activity)
            return ("Do you want to cancel %s? "):format(_.name(activity))
        end,
    },

    Resting = {
        Start = "You lie down to rest.",
        Finish = "You finished taking a rest.",
        DropOffToSleep = "After a short while, you drop off to sleep.",
    },

    Eating = {
        Start = {
            Normal = function(actor, food)
                return ("%s start%s to eat %s."):format(_.name(actor), _.s(actor), _.name(food, nil, 1))
            end,
            InSecret = function(actor, food)
                return ("%s start%s to eat %s in secret."):format(_.name(actor), _.s(actor), _.name(food, nil, 1))
            end,
            Mammoth = "Let's eatammoth.",
        },
        Finish = function(actor, food)
            return ("%s %s finished eating %s."):format(_.name(actor), _.has(actor), _.name(food, nil, 1))
        end,
    },

    Sex = {
        Gender = {
            Male = "boy",
            Female = "girl",
        },
        TakesClothesOff = function(entity)
            return ("%s begin%s to take %s clothes off."):format(_.name(entity), _.s(entity), _.his(entity))
        end,
        Dialog = { "Yes!", "Ohhh", "*gasp*", "*rumble*", "come on!" },
        DialogAfter = { "You are awesome!", "Oh my god....", "Okay, okay, you win!", "Holy...!" },
        GetsFurious = function(entity)
            return ("%s gets furious, \"And you think you can just run away?\""):format(_.name(entity))
        end,
        SpareLife = function(actor, partner)
            return ("\"I-I don't really know that %s. Please spare my life!\""):format(
                _.loc("Elona.Activity.Sex.Gender." .. _.gender(actor))
            )
        end,
        Take = "Here, take this.",
        TakeAllIHave = "Take this money, it's all I have!",
    },
}
