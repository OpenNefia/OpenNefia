Elona.MapTransfer = {
    Leave = {
        DeliveredToYourHome = "You were delivered to your home.",
        Entered = function(mapEntity)
            return ("You entered %s."):format(mapEntity)
        end,
        ReturnedTo = function(mapEntity)
            return ("You returned to %s."):format(mapEntity)
        end,
        Left = function(mapEntity)
            return ("You left %s."):format(mapEntity)
        end,
        BurdenedByCargo = "The weight of your cargo burdens your traveling speed.",
    },
    Travel = {
        TimePassed = function(days, hours, lastTownName)
            return ("%s day%s and %s hour%s have passed since you left %s."):format(
                days,
                _.s(days),
                hours,
                _.s(hours),
                lastTownName
            )
        end,

        Walked = {
            You = function(travelDistance)
                return ("You've walked about %s miles and have gained experience."):format(travelDistance)
            end,
            YouAndAllies = function(travelDistance)
                return ("You and your allies have walked about %s miles and have gained experience."):format(
                    travelDistance
                )
            end,
        },
    },
}
