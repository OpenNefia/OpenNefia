Elona.Potion = {
    ItemName = {
        Aphrodisiac = "(媚薬混入)",
        Poisoned = "(毒物混入)",
    },
    Drinks = function(entity, item)
        return ("%s%sを飲み干した。"):format(_.sore_wa(entity), _.name(item, nil, 1))
    end,
    Thrown = {
        Shatters = "それは地面に落ちて砕けた。",
    },
}
