Elona.Chara = {
    PlaceFailure = {
        Ally = function(target)
            return ("%sとはぐれた。"):format(_.name(target))
        end,
        Other = function(target)
            return ("%sは何かに潰されて息絶えた。"):format(_.name(target))
        end,
    },
}
