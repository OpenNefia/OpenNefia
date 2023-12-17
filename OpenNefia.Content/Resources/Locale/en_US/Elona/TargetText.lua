Elona.TargetText = {
    DangerLevel = {
        ["0"] = function(target)
            return ("You can absolutely beat %s with your eyes closed and arms crossed."):format(_.him(target))
        end,
        ["1"] = function(target)
            return ("You bet you can beat %s with your eyes closed."):format(_.him(target))
        end,
        ["2"] = function(target)
            return ("%s %s an easy opponent."):format(_.he(target), _.is(target))
        end,
        ["3"] = "You will probably win.",
        ["4"] = "Won't be an easy fight.",
        ["5"] = "The opponent looks quite strong.",
        ["6"] = function(target)
            return ("%s %s at least twice stronger than you."):format(_.he(target), _.is(target))
        end,
        ["7"] = "You will get killed unless miracles happen.",
        ["8"] = "You will get killed, a hundred percent sure.",
        ["9"] = function(target)
            return ("%s can mince you with %s eyes closed."):format(_.he(target), _.his(target))
        end,
        ["10"] = function(target)
            return ("If %s is a giant, you are less than a dropping of an ant."):format(_.he(target))
        end,
    },
    ItemOnCell = {
        And = " and ",

        MoreThanThree = function(itemCount)
            return ("There are %s items lying here."):format(itemCount)
        end,

        Item = function(itemNames)
            return ("You see %s here."):format(itemNames)
        end,
        Construct = function(itemNames)
            return ("%s is constructed here."):format(itemNames)
        end,
        NotOwned = function(itemNames)
            return ("You see %s placed here."):format(itemNames)
        end,
    },
}
