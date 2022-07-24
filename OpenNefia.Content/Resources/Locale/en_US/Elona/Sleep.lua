Elona.Sleep = {
    NotSleepy = "You don't feel sleepy yet.",
    ButYouCannot = "But you can't sleep right now.",

    Activity = {
        Start = {
            LieDown = "You lie down.",
            StartToCamp = "You start to camp.",
        },
        Finish = "You fall asleep.",
    },

    WakeUp = {
        SoSo = "You wake up feeling so so.",
        Good = function(grownCount)
            return ("You wake up feeling good. Your potential increases. (Total:%s%%)"):format(grownCount)
        end,
    },
}
