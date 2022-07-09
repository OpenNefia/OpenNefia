Elona.Food = {
    Cook = function(_1, _2, _3)
        return ("You cook %s with %s and make %s."):format(_1, _2, _3)
    end,
    EatStatus = {
        Good = function(_1)
            return ("%s feel%s good."):format(_.name(_1), _.s(_1))
        end,
        Bad = function(_1)
            return ("%s feel%s bad."):format(_.name(_1), _.s(_1))
        end,
        CursedDrink = function(_1)
            return ("%s feel%s grumpy."):format(_.name(_1), _.s(_1))
        end,
    },
    Nutrition = {
        Bloated = {
            "Phew! You are pretty bloated.",
            "You've never eaten this much before!",
            "Your stomach is unbelievably full!",
        },
        Satisfied = {
            "You are satisfied!",
            "This hearty meal has filled your stomach.",
            "You really ate!",
            "You pat your stomach contentedly.",
        },
        Hungry = {
            "You are still a bit hungry.",
            "Not enough...",
            "You want to eat more.",
            "Your stomach is still somewhat empty.",
        },
        Normal = { "You can eat more.", "You pat your stomach.", "You satisfied your appetite a little." },
        VeryHungry = { "No, it was not enough at all.", "You still feel very hungry.", "You aren't satisfied." },
        Starving = {
            "It didn't help you from starving!",
            "It prolonged your death for seconds.",
            "Empty! Your stomach is still empty!",
        },
    },
    Message = {
        Quality = {
            Bad = { "Boy, it gives your stomach trouble!", "Ugh! Yuk!", "Awful taste!!" },
            SoSo = { "Uh-uh, the taste is so so.", "The taste is not bad." },
            Good = { "It tasted good.", "Decent meal." },
            Great = { "Delicious!", "Gee what a good taste!", "It tasted pretty good!" },
            Delicious = { "Wow! Terrific food!", "Yummy! Absolutely yummy!", "It tasted like seventh heaven!" },
        },
        Uncooked = { "It doesn't taste awful but...", "Very boring food." },
        Human = {
            Delicious = "Delicious!",
            Dislike = "Eeeek! It's human flesh!",
            Like = "It's your favorite human flesh!",
            WouldHaveRatherEaten = "You would've rather eaten human flesh.",
        },
        RawGlum = function(_1)
            return ("%s looks glum."):format(_.name(_1))
        end,
        Rotten = "Ugh! Rotten food!",
        Ability = {
            Deteriorates = function(_1, _2)
                return ("%s%s %s deteriorates."):format(_.name(_1), _.his_named(_1), _2)
            end,
            Develops = function(_1, _2)
                return ("%s%s %s develops."):format(_.name(_1), _.his_named(_1), _2)
            end,
        },
        SustainsGrowth = function(_1, _2)
            return ("%s%s %s enters a period of rapid growth."):format(_.name(_1), _.his_named(_1), _2)
        end,
    },
    NotAffectedByRotten = function(_1)
        return ("But %s%s stomach isn't affected."):format(_.name(_1), _.his_named(_1))
    end,
    PassedRotten = { "Yuck!!", "....!!", "W-What...", "Are you teasing me?", "You fool!" },
}
