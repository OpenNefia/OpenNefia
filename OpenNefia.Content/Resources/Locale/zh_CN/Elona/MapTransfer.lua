Elona.MapTransfer = {
    Leave = {
        DeliveredToYourHome = "你被送到了家里。",
        Entered = function(mapEntity)
            return ("进入了%s。"):format(_.name(mapEntity, true))
        end,
        ReturnedTo = function(mapEntity)
            return ("返回了%s。"):format(_.name(mapEntity, true))
        end,
        Left = function(mapEntity)
            return ("离开了%s。"):format(_.name(mapEntity, true))
        end,
        BurdenedByCargo = "由于货物太重，你的行动速度明显变慢！",
    },
    Travel = {
        TimePassed = function(days, hours, lastTownName, date)
            return ("从%s出发后，经过%s天%s小时的旅行，到达了%s。"):format(
                _.format_date(date),
                days,
                hours,
                lastTownName
            )
        end,

        Walked = {
            You = function(travelDistance)
                return ("你走了%s英里的路程，积累了经验。"):format(travelDistance)
            end,
            YouAndAllies = function(travelDistance)
                return ("你和你的队友们走了%s英里的路程，积累了经验。"):format(
                    travelDistance
                )
            end,
        },
    },
}