Elona.Item = {
    ItemName = {
        EternalForce = "永恒之力",
        LostProperty = "(遗失物)",
        UseInterval = function(hours)
            return ("(%s小时)"):format(hours)
        end,
        FromEntity = function(name)
            return ("%s的"):format(name)
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
            Default = "个",
            Clothing = "件",
            Books = "本",
            Flats = "枚",
            Rods = "支",
            Doses = "服",
            Scrolls = "卷",
            Pairs = "双",
            SmallAnimals = "只",
        },
    },
}