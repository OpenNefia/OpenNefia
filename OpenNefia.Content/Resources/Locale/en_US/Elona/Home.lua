Elona.Home = {
    Map = {
        Name = "Your Home",
        Description = "It's your sweet home.",
    },
    ItemName = {
        Deed = function(home_name)
            return ("of %s"):format(home_name)
        end,
    },
    WelcomeHome = {
        _.quote "Welcome home!",
        _.quote "Hey, dear.",
        _.quote "You're back!",
        _.quote "I was waiting for you.",
        _.quote "Nice to see you again.",
    },
    Design = {
        Help = "Left click to place the tile, right click to pick the tile under your mouse cursor, movement keys to move current position, hit the enter key to show the list of tiles, hit the cancel key to exit.",
    },
    Rank = {
        Change = function(furnitureValue, heirloomValue, prevRank, newRank, newTitle, rankName)
            return ("Furniture Value:%s Heirloom Value:%s Home Rank:%s->%s Your home is now known as <%s>."):format(
                furnitureValue,
                heirloomValue,
                prevRank,
                newRank,
                newTitle
            )
        end,
        Window = {
            Place = function(ordinal)
                return ("%s"):format(ordinal)
            end,
            Star = "*",
            Title = "Home Value",
            Topic = {
                HeirloomRank = "Heirloom Rank",
                Value = "Value",
            },
            Type = {
                Base = "Base",
                Deco = "Deco",
                Heir = "Heir",
                Total = "Total",
            },
        },
    },
}
