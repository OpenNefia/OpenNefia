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
    Vomits = function(_1)
        return ("%sは吐いた。"):format(_.name(_1))
    end,
}
