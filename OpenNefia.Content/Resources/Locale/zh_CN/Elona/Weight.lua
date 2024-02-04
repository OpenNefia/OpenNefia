Elona.Weight = {
    Weight = {
        Gain = function(_1)
            return ("%s变胖了。"):format(_.name(_1))
        end,
        Lose = function(_1)
            return ("%s变瘦了。"):format(_.name(_1))
        end,
    },
}