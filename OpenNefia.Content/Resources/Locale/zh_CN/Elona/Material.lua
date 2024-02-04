Elona.Material = {
    ItemDescription = {
        ItIsMadeOf = function(item, materialName)
            return ("%s是由%s制成的"):format(_.he(item), materialName)
        end,
    },
}