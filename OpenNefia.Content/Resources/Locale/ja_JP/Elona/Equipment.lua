Elona.Equipment = {
    ArmorClass = {
        Light = "(軽装備)",
        Medium = "(中装備)",
        Heavy = "(重装備)",
    },

    YouChangeYourEquipment = "装備を変更した。",

    Layer = {
        Window = {
            Title = "装備品",
        },

        Topic = {
            CategoryName = "部位/装備品名称",
            Weight = "重さ",
        },

        Stats = {
            EquipWeight = "装備重量",
            HitBonus = "命中修正",
            DamageBonus = "ダメージ修正",
        },

        MainHand = "利手",
    },

    Suitability = {
        TwoHand = {
            FitsWell = function(actor, target, item)
                return ("装備中の%sは両手にしっくりとおさまる。"):format(_.name(item))
            end,
            TooLight = function(actor, target, item)
                return ("装備中の%sは両手持ちにはやや軽すぎる。"):format(_.name(item))
            end,
        },
        DualWield = {
            TooHeavy = {
                MainHand = function(actor, target, item)
                    return ("装備中の%sは利手で扱うにも重すぎる。"):format(_.name(item))
                end,
                SubHand = function(actor, target, item)
                    return ("装備中の%sは片手で扱うには重すぎる。"):format(_.name(item))
                end,
            },
        },
        Riding = {
            TooHeavy = function(actor, target, item)
                return ("装備中の%sは乗馬中に扱うには重過ぎる。"):format(_.name(item))
            end,
        },
    },
}
