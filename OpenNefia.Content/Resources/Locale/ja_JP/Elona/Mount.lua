Elona.Mount = {
    Start = {
        Problems = {
            CanOnlyRideAlly = "仲間にしか騎乗できない。",
            CannotRideClient = "護衛対象には騎乗できない。",
            RideSelf = function(rider)
                return ("%s自分に乗ろうとした。"):format(_.kare_wa(rider))
            end,
            IsStayer = "その仲間はこの場所に滞在中だ。",
            IsCurrentlyRiding = function(rider, mount)
                return ("現在%sは%sに騎乗している。"):format(_.name(rider), _.name(mount))
            end,
        },

        YouRide = function(rider, mount, mountPrevSpeed, mountNewSpeed)
            return ("%s%sに騎乗した(%sの速度: %s→%s"):format(
                _.sore_wa(rider),
                _.name(mount),
                _.name(mount),
                mountPrevSpeed,
                mountNewSpeed
            )
        end,

        Suitability = {
            Good = "この生物は乗馬用にちょうどいい！",
            Bad = function(rider, mount)
                return ("この生物は%sを乗せるには非力すぎる。"):format(_.name(rider))
            end,
        },

        Dialog = {
            _.quote "うぐぅ",
            _.quote "ダイエットしてよ…",
            _.quote "いくよ！",
            _.quote "やさしくしてね♪",
        },
    },

    Stop = {
        NoPlaceToGetOff = "降りるスペースがない。",
        YouDismount = function(rider, mount)
            return ("%s%sから降りた。"):format(_.sore_wa(rider), _.name(mount))
        end,
        DismountCorpse = function(rider, mount)
            return ("%s%sの死体から降りた。"):format(_.kare_wa(rider), _.name(mount, true))
        end,
    },

    Movement = {
        InterruptActivity = function(rider, mount)
            return ("%s%sを睨み付けた。"):format(_.kare_wa(mount), _.name(rider))
        end,
    },
}
