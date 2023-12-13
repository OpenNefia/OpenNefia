Elona.Food = {
    ItemName = {
        Rotten = "rotten",
    },
    Cooking = {
        DoNotKnow = function(user)
            return ("%s %s know how to cook."):format(_.name(user), _.does_not(user))
        end,
        YouCook = function(user, oldFoodName, toolEntity, newFoodEntity)
            return ("%s cook%s %s with %s and make%s %s."):format(
                _.name(user),
                _.s(user),
                oldFoodName,
                _.name(toolEntity, 1),
                _.s(user),
                _.name(newFoodEntity, 1)
            )
        end,
    },
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
                return ("%s %s deteriorates."):format(_.possessive(_1), _2)
            end,
            Develops = function(_1, _2)
                return ("%s %s develops."):format(_.possessive(_1), _2)
            end,
        },
    },
    NotAffectedByRotten = function(_1)
        return ("But %s stomach isn't affected."):format(_.possessive(_1))
    end,
    PassedRotten = {
        _.quote "Yuck!!",
        _.quote "....!!",
        _.quote "W-What...",
        _.quote "Are you teasing me?",
        _.quote "You fool!",
    },

    Harvesting = {
        ItemName = {
            Grown = function(weight)
                return ("grown %s"):format(weight)
            end,
        },
        Weight = {
            ["0"] = "extremely mini",
            ["1"] = "small",
            ["2"] = "handy",
            ["3"] = "rather big",
            ["4"] = "huge",
            ["5"] = "pretty huge",
            ["6"] = "monstrous-size",
            ["7"] = "bigger than a man",
            ["8"] = "legendary-size",
            ["9"] = "heavier than an elephant",
        },
    },
}
