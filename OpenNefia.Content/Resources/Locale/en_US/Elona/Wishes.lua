Elona.Wishes = {
    Prompt = function(wisher)
        return ("What %s %s wish for? "):format(_.does(wisher), _.name(wisher))
    end,
    YouWish = function(wisher, wish)
        return _.quote(("%s!!"):format(wish))
    end,

    ItIsSoldOut = "It's sold out.",

    SomethingAppears = {
        Normal = function(wisher, item)
            return ("%s appear%s."):format(_.name(item), _.s(item))
        end,
        FromNowhere = function(wisher, item)
            return ("%s appear%s from nowhere."):format(_.name(item), _.s(item))
        end,
        FallsDown = function(wisher, item)
            ("%s fall%s from the sky!"):format(_.name(item), _.s(item))
        end,
    },

    General = {
        Card = {
            Keyword = "card",
        },
        Figure = {
            Keyword = "figure",
        },
        Item = {
            Keyword = "item",
        },
        Skill = {
            Keyword = "skill",

            Gain = function(wisher, skillName)
                return ("%s learn%s %s!"):format(_.name(wisher), _.s(wisher), skillName)
            end,
            Improve = function(wisher, skillName)
                return ("%s %s skill improves!"):format(_.possessive(wisher), skillName)
            end,
        },
        Summon = {
            Keyword = "summon",
        },
    },
}
