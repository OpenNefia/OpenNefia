Elona.Quality = {
    Names = {
        Bad = "粗糙的",
        Normal = "普通的",
        Good = "良好的",
        Great = "优秀的",
        God = "神级的",
        Unique = "独特的",
    },
    Brackets = {
        Great = function(name)
            return ("【%s】"):format(name)
        end,
        God = function(name)
            return ("【%s】"):format(name)
        end,
    },
}