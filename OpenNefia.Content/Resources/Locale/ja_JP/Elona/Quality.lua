Elona.Quality = {
    Names = {
        Bad = "粗悪",
        Good = "良質",
        Great = "高品質",
        Miracle = "奇跡",
        Godly = "神器",
        Special = "特別",
    },
    Brackets = {
        Great = function(name)
            return ("『%s』"):format(name)
        end,
        God = function(name)
            return ("《%s》"):format(name)
        end,
    },
}
