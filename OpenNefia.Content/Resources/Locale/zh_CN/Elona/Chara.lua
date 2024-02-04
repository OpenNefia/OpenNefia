Elona.Chara = {
    PlaceFailure = {
        Ally = function(target)
            return ("与%s失散了。"):format(_.name(target))
        end,
        Other = function(target)
            return ("与%s被某个东西碾压而死。"):format(_.name(target))
        end,
    },
}