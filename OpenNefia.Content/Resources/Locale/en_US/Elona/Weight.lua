Elona.Weight = {
    Weight = {
        Gain = function(_1)
            return ("%s gain%s weight."):format(_.name(_1), _.s(_1))
        end,
        Lose = function(_1)
            return ("%s lose%s weight."):format(_.name(_1), _.s(_1))
        end,
    },
}
