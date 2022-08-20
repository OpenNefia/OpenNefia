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
                _.loc("Elona.Gender.Names." .. _.gender(actor) .. ".Informal")
            )
        end,
        Take = "Here, take this.",
        TakeAllIHave = "Take this money, it's all I have!",
    },

    Performing = {
        Dialog = {
            Angry = {
                _.quote "Boo boo!",
                _.quote "Shut it!",
                _.quote "What are you doing!",
                _.quote "You can't play shit.",
            },
            Disinterest = { _.quote "Boring.", _.quote "I've heard this before.", _.quote "This song again?" },
            Interest = {
                function(audience)
                    return ("%s clap%s."):format(_.name(audience), _.s(audience))
                end,
                function(audience, actor)
                    return ("%s listen%s to %s music joyfully."):format(
                        _.name(audience),
                        _.s(audience),
                        _.possessive(actor)
                    )
                end,
                _.quote "Bravo!",
                _.quote "Nice song.",
                _.quote "Scut!",
                function(audience)
                    return ("%s %s excited!"):format(_.name(audience), _.is(audience))
                end,
            },
        },
        GetsAngry = function(audience)
            return ("%s get%s angry."):format(_.name(audience), _.s(audience))
        end,
        Quality = {
            ["0"] = "It is a waste of time.",
            ["1"] = "Almost everyone ignores you.",
            ["2"] = "You need to practice lot more.",
            ["3"] = "You finish your performance.",
            ["4"] = "Not good.",
            ["5"] = "People seem to like your performance.",
            ["6"] = "Your performance is successful.",
            ["7"] = "Wonderful!",
            ["8"] = "Great performance. Everyone cheers you.",
            ["9"] = "A Legendary stage!",
        },
        Sound = {
            Cha = "*Cha*",
            Random = { "*Tiki*", "*Dan*", "*Lala*" },
        },
        Start = function(actor, instrument)
            return ("%s start%s to play %s."):format(_.name(actor), _.s(actor), _.name(instrument))
        end,
        ThrowsRock = function(audience)
            return ("%s throw%s a rock."):format(_.name(audience), _.s(audience))
        end,
        Tip = function(actor, tips)
            return ("The audience gives %s total of %s gold pieces."):format(_.name(actor), tips)
        end,
    },
}
