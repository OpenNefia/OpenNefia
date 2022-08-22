Elona.Quality = {
    Names = {
        Bad = "粗悪",
        Normal = "良質",
        Good = "高品質",
        Great = "奇跡",
        God = "神器",
        Unique = "特別",
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
