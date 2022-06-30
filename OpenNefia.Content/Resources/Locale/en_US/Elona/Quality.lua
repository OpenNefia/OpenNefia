Elona.Quality = {
    Names = {
        Bad = "bad",
        Good = "good",
        Great = "great",
        Miracle = "miracle",
        Godly = "godly",
        Special = "special",
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
