Elona.Hunger = {
    Anorexia = {
        Develops = function(_1)
            return ("%sは拒食症になった。"):format(_.name(_1))
        end,
        RecoversFrom = function(_1)
            return ("%sの拒食症は治った。"):format(_.name(_1))
        end,
    },
    Status = {
        Hungry = { "腹がすいてきた。", "空腹になった。", "さて何を食べようか。" },
        VeryHungry = { "空腹で目が回りだした…", "すぐに何かを食べなくては…" },
        Starving = {
            "このままだと餓死してしまう！",
            "腹が減ってほとんど死にかけている。",
        },
    },
    Indicator = {
        ["0"] = "餓死中",
        ["1"] = "飢餓",
        ["2"] = "空腹",
        ["3"] = "空腹",
        ["4"] = "空腹",
        ["5"] = "",
        ["6"] = "",
        ["7"] = "",
        ["8"] = "",
        ["9"] = "",
        ["10"] = "満腹",
        ["11"] = "満腹",
        ["12"] = "食過ぎ",
    },
    Vomits = function(_1)
        return ("%sは吐いた。"):format(_.name(_1))
    end,
}
