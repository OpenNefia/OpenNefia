Elona.Weather = {
    Feat = {
        DrawCloud = function(entity)
            return ("%s雨雲を引き寄せた。"):format(_.kare_wa(entity))
        end,
    },

    Changes = "天候が変わった。",

    Types = {
        Etherwind = {
            Starts = "エーテルの風が吹き始めた。すぐに避難しなくては。",
            Stops = "エーテルの風は止んだ。",
        },
        Rain = {
            Starts = "雨が降り出した。",
            Stops = "雨は止んだ。",
            BecomesHeavier = "雨が本格的に降り出した。",
        },
        HardRain = {
            Starts = "突然どしゃぶりになった。",
            BecomesLighter = "雨は小降りになった。",
            Travel = {
                Hindered = {
                    "雨が激しすぎてどこを歩いているかもわからない！",
                    "あまりにも視界が悪すぎる。",
                    "豪雨のせいで前が全く見えない。",
                },
                Sound = { " *びしゃ* ", " *ザブッ* ", " *パシャッ* ", " *ざぶ* " },
            },
        },
        Snow = {
            Starts = "雪が降ってきた。",
            Stops = "雪は止んだ。",
            Travel = {
                Hindered = {
                    "積雪のせいで旅程が遅れている。",
                    "雪道を進むのは大変な苦労だ。",
                    "深い雪に脚をとられている。",
                },
                Eat = "空腹のあまり、あなたは積もっている雪を腹にかきこんだ。",
                Sound = { " *ずぶっ* ", " *ザシュ* ", " *ズボ* ", " *ズサッ* " },
            },
        },
    },
}
