Elona.MapTransfer = {
    Leave = {
        DeliveredToYourHome = "You were delivered to your home.",
        Entered = function(mapEntity)
            return ("You entered %s."):format(_.name(mapEntity, true))
        end,
        ReturnedTo = function(mapEntity)
            return ("You returned to %s."):format(_.name(mapEntity, true))
        end,
        Left = function(mapEntity)
            return ("You left %s."):format(_.name(mapEntity, true))
        end,
        BurdenedByCargo = "The weight of your cargo burdens your traveling speed.",
    },
    Travel = {
        TimePassed = function(days, hours, lastTownName)
            return ("%s day%s and %s hour%s have passed since you left %s."):format(
                days,
                _.plural(days),
                hours,
                _.plural(hours),
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
