Elona.TargetText = {
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
