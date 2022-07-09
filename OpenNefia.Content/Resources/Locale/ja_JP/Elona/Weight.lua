Elona.Weight = {
    Weight = {
        Gain = function(_1)
            return ("%sは太った。"):format(_.name(_1))
        end,
        Lose = function(_1)
            return ("%sは痩せた。"):format(_.name(_1))
        end,
    },
}
