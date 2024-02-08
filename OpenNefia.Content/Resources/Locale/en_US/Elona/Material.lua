Elona.Material = {
    ItemDescription = {
        ItIsMadeOf = function(item, materialName)
            return ("%s %s made of %s."):format(_.he(item), _.is(item), materialName)
        end,
    },
}
