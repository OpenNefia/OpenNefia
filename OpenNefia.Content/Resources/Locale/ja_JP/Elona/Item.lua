Elona.Item = {
    ItemName = {
        EternalForce = "エターナルフォース",
        LostProperty = "(落し物)",
        UseInterval = function(hours)
            return ("(%s時間)"):format(hours)
        end,
        FromEntity = function(name)
            return ("%sの"):format(name)
        end,
    },
    NameModifiers = {
        Great = function(name)
            return ("『%s』"):format(name)
        end,
        God = function(name)
            return ("《%s》"):format(name)
        end,
        Article = function(name)
            return ("%s"):format(name)
        end,
    },

    Japanese = {
        Counters = {
            Default = "個",
            Clothing = "着",
            Books = "冊",
            Flats = "枚",
            Rods = "本",
            Doses = "服",
            Scrolls = "巻",
            Pairs = "対",
            SmallAnimals = "匹",
        },
    },
}
