Elona.Sleep = {
    NotSleepy = "还不困。",
    ButYouCannot = "但是，你突然想起了重要的事情，不能睡觉。",
    YouNeedToSleep = "你需要睡觉。",
    Activity = {
        Start = {
            LieDown = "开始准备睡觉。",
            StartToCamp = "开始准备露营。",
        },
        Finish = "你陷入了沉睡之中。",
    },
    WakeUp = {
        SoSo = "醒来后感觉还好。",
        Good = function(grownCount)
            return ("醒来后感觉很舒服。潜力增长了(总计%s%%)。"):format(grownCount)
        end,
    },
    Indicator = {
        Light = "可以睡觉",
        Moderate = "需要睡觉",
        Heavy = "需要睡觉",
    },
}