Elona.Weather = {
    Feat = {
        DrawCloud = function(entity)
            return ("%s吸引了云雨。"):format(_.kare_wa(entity))
        end,
    },

    Changes = "天气发生了变化。",

    Types = {
        Etherwind = {
            Starts = "以太风开始吹起。必须立即避难。",
            Stops = "以太风停止了。",
        },
        Rain = {
            Starts = "开始下雨了。",
            Stops = "雨停了。",
            BecomesHeavier = "雨越下越大。",
        },
        HardRain = {
            Starts = "突然暴雨倾盆。",
            BecomesLighter = "雨变小了。",
            Travel = {
                Hindered = {
                    "雨下得太大，完全看不清自己在走哪里！",
                    "视线太差，几乎什么都看不见。",
                    "因为暴雨完全看不见前方。",
                },
                Sound = { " *嗖* ", " *滑沙* ", " *拍杂* ", " *滴答* " },
            },
        },
        Snow = {
            Starts = "开始下雪了。",
            Stops = "雪停了。",
            Travel = {
                Hindered = {
                    "因为积雪原因，旅程受到了延迟。",
                    "行走雪地非常艰难。",
                    "因为深厚的雪而跌倒。",
                },
                Eat = "因为饥饿，你吃下了积雪。",
                Sound = { " *噗通* ", " *哗喆* ", " *噗嗖* ", " *噗沙* " },
            },
        },
    },
}