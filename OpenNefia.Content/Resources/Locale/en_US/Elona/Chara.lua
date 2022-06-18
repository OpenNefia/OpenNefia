Elona.Chara = {
    PlaceFailure = {
        Ally = function(target)
            return ("%s loses %s way."):format(_.name(target), _.his(target))
        end,
        Other = function(target)
            return ("%s is killed."):format(_.name(target))
        end,
    },
}
