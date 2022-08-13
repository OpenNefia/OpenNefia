Elona.Sleep = {
    NotSleepy = "まだ眠たくない。",
    ButYouCannot = "しかし、大事な用を思い出して飛び起きた。",
    YouNeedToSleep = "あなたは眠りを必要としている。",

    Activity = {
        Start = {
            LieDown = "寝る仕度を始めた。",
            StartToCamp = "露営の準備を始めた。",
        },
        Finish = "あなたは眠り込んだ。",
    },

    WakeUp = {
        SoSo = "まあまあの目覚めだ。",
        Good = function(grownCount)
            return ("心地よい目覚めだ。潜在能力が伸びた(計%s%%)。"):format(grownCount)
        end,
    },

    Indicator = {
        Light = "睡眠可",
        Moderate = "要睡眠",
        Heavy = "要睡眠",
    },
}
