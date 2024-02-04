Elona.Mount = {
    Start = {
        Problems = {
            CanOnlyRideAlly = "只能骑乘同伴。",
            CannotRideClient = "无法骑乘护卫对象。",
            RideSelf = function(rider)
                return ("%s试图骑在自己身上。"):format(_.kare_wa(rider))
            end,
            IsStayer = "该同伴当前滞留在此地。",
            IsCurrentlyRiding = function(rider, mount)
                return ("%s目前骑在%s上。"):format(_.name(rider), _.name(mount))
            end,
        },

        YouRide = function(rider, mount, mountPrevSpeed, mountNewSpeed)
            return ("%s骑上了%s（速度从%s变为%s）"):format(
                _.sore_wa(rider),
                _.name(mount),
                _.name(mount),
                mountPrevSpeed,
                mountNewSpeed
            )
        end,

        Suitability = {
            Good = "这个生物非常适合骑乘！",
            Bad = function(rider, mount)
                return ("这个生物太弱了，无法载住%s。"):format(_.name(rider))
            end,
        },

        Dialog = {
            _.quote "噗呜",
            _.quote "请减肥吗……",
            _.quote "开始吧！",
            _.quote "请轻柔一点♪",
        },
    },

    Stop = {
        NoPlaceToGetOff = "没有下车的空间。",
        YouDismount = function(rider, mount)
            return ("%s从%s上下来。"):format(_.sore_wa(rider), _.name(mount))
        end,
        DismountCorpse = function(rider, mount)
            return ("%s从%s的尸体上下来。"):format(_.kare_wa(rider), _.name(mount, true))
        end,

        Dialog = {
            _.quote "呼",
            _.quote "乘坐舒适吗？",
            _.quote "好累……",
            _.quote "随时欢迎再次乘坐♪",
        },
    },

    Movement = {
        InterruptActivity = function(rider, mount)
            return ("%s盯着%s看。"):format(_.kare_wa(mount), _.name(rider))
        end,
    },
}