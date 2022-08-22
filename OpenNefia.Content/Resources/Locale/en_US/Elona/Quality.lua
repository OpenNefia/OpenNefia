Elona.Quality = {
    Names = {
        Bad = "bad",
        Normal = "good",
        Good = "great",
        Great = "miracle",
        God = "godly",
        Unique = "special",
    },
    Brackets = {
        Great = function(name)
            return ("<%s>"):format(name)
        end,
        God = function(name)
            return ("{%s}"):format(name)
        end,
    },
}
