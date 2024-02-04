Elona.Potion = {
    ItemName = {
        Aphrodisiac = "(混入了媚药)",
        Poisoned = "(混入了毒物)",
    },
    Drinks = function(entity, item)
        return ("%s喝掉了%s。"):format(_.sore_wa(entity), _.name(item, nil, 1))
    end,
    Thrown = {
        Shatters = "它摔在地上破碎了。",
    },
}