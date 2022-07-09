Elona.Drinkable = {
    Drinks = function(entity, item)
        return ("%s%sを飲み干した。"):format(_.sore_wa(entity), _.name(item))
    end,
    Thrown = {
        Shatters = "それは地面に落ちて砕けた。",
    },
}
