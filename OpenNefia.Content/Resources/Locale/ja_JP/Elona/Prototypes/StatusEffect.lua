OpenNefia.Prototypes.Elona.StatusEffect.Elona = {
    Bleeding = {
        Apply = function(chara)
            return ("%sは血を流し始めた。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%sの出血は止まった。"):format(_.name(chara))
        end,
        Indicator = {
            ["0"] = "切り傷",
            ["1"] = "出血",
            ["2"] = "大出血",
        },
    },
    Blindness = {
        Apply = function(chara)
            return ("%sは盲目になった。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%sは盲目から回復した。"):format(_.name(chara))
        end,
        Blind = "盲目",
    },
    Confusion = {
        Apply = function(chara)
            return ("%sは混乱した。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%sは混乱から回復した。"):format(_.name(chara))
        end,
        Indicator = {
            ["0"] = "混乱",
        },
    },
    Dimming = {
        Apply = function(chara)
            return ("%sは朦朧とした。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%sの意識ははっきりした。"):format(_.name(chara))
        end,
        Indicator = {
            ["0"] = "朦朧",
            ["1"] = "混濁",
            ["2"] = "気絶",
        },
    },
    Drunk = {
        Apply = function(chara)
            return ("%sは酔っ払った。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%sの酔いは覚めた。"):format(_.name(chara))
        end,
        Indicator = {
            ["0"] = "酔払い",
        },
    },
    Fear = {
        Apply = function(chara)
            return ("%sは恐怖に侵された。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%sは恐怖から立ち直った。"):format(_.name(chara))
        end,
        Indicator = {
            ["0"] = "恐怖",
        },
    },
    Insanity = {
        Apply = function(chara)
            return ("%sは気が狂った。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%sは正気に戻った。"):format(_.name(chara))
        end,
        Insane = {
            ["0"] = "不安定",
            ["1"] = "狂気",
            ["2"] = "崩壊",
        },
    },
    Paralysis = {
        Apply = function(chara)
            return ("%sは麻痺した。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%sは麻痺から回復した。"):format(_.name(chara))
        end,
        Indicator = {
            ["0"] = "麻痺",
        },
    },
    Poison = {
        Apply = function(chara)
            return ("%sは毒におかされた。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%sは毒から回復した。"):format(_.name(chara))
        end,
        Indicator = {
            ["0"] = "毒",
            ["1"] = "猛毒",
        },
    },
    Sick = {
        Apply = function(chara)
            return ("%sは病気になった。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%sの病気は治った。"):format(_.name(chara))
        end,
        Indicator = {
            ["0"] = "病気",
            ["1"] = "重病",
        },
    },
    Sleep = {
        Apply = function(chara)
            return ("%sは眠りにおちた。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%sは心地よい眠りから覚めた。"):format(_.name(chara))
        end,
        Indicator = {
            ["0"] = "睡眠",
            ["1"] = "爆睡",
        },
    },
    Choking = {
        Indicator = {
            ["0"] = "窒息",
        },
    },
    Fury = {
        Indicator = {
            ["0"] = "激怒",
            ["1"] = "狂乱",
        },
    },
    Gravity = {
        Indicator = {
            ["0"] = "重力",
        },
    },
    Wet = {
        Indicator = {
            ["0"] = "濡れ",
        },
    },
}
