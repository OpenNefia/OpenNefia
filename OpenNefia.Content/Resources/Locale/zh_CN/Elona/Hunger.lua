Elona.Hunger = {
    Anorexia = {
        Develops = function(_1)
            return ("%s患上了厌食症。"):format(_.name(_1))
        end,
        RecoversFrom = function(_1)
            return ("%s的厌食症已治愈。"):format(_.name(_1))
        end,
    },
    Status = {
        Hungry = { "开始感到肚子饿了。", "变得饥肠辘辘。", "该吃点什么呢？" },
        VeryHungry = { "饿得头晕目眩…", "必须立即吃点东西…" },
        Starving = {
            "再这样下去会饿死的！",
            "饥肠辘辘，几近于死。",
        },
    },
    Indicator = {
        ["0"] = "濒死饥饿",
        ["1"] = "饥饿",
        ["2"] = "空腹",
        ["3"] = "空腹",
        ["4"] = "空腹",
        ["5"] = "",
        ["6"] = "",
        ["7"] = "",
        ["8"] = "",
        ["9"] = "",
        ["10"] = "饱食",
        ["11"] = "饱食",
        ["12"] = "吃得过多",
    },
    Vomits = function(_1)
        return ("%s呕吐了。"):format(_.name(_1))
    end,
}