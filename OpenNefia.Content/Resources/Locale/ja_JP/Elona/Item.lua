Elona.Item = {
    ItemName = {
        LostProperty = "(落し物)",
        UseInterval = function(hours)
            return ("(%s時間)"):format(hours)
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
}
