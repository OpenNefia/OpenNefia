Elona.MapTransfer = {
    Leave = {
        DeliveredToYourHome = "あなたは家まで運ばれた。",
        Entered = function(mapEntity)
            return ("%sに入った。"):format(mapEntity)
        end,
        ReturnedTo = function(mapEntity)
            return ("%sに戻った。"):format(mapEntity)
        end,
        Left = function(mapEntity)
            return ("%sを後にした。"):format(mapEntity)
        end,
        BurdenedByCargo = "荷車の重量超過でかなり鈍足になっている！ ",
    },
    Travel = {
        TimePassed = function(days, hours, lastTownName, date)
            return ("%sに%sを発ってから、%s日と%s時間の旅を終えた。"):format(
                date,
                lastTownName,
                days,
                hours
            )
        end,

        Walked = {
            You = function(travelDistance)
                return ("あなたは%sマイルの距離を歩き、経験を積んだ。"):format(travelDistance)
            end,
            YouAndAllies = function(travelDistance)
                return ("あなたとその仲間は%sマイルの距離を歩き、経験を積んだ。"):format(
                    travelDistance
                )
            end,
        },
    },
}
