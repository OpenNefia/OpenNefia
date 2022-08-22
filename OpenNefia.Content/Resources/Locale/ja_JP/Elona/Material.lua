Elona.Material = {
    ItemDescription = {
        ItIsMadeOf = function(item, materialName)
            return ("%s%sで作られている"):format(_.kare_wa(item), materialName)
        end,
    },
}
