Elona.Potion = {
    ItemName = {
        Aphrodisiac = "(Aphrodisiac)",
        Poisoned = "(Poisoned)",
    },
    Drinks = function(entity, item)
        return ("%s drink%s %s."):format(_.name(entity), _.s(entity), _.name(item, nil, 1))
    end,
    Thrown = {
        Shatters = "It falls on the ground and shatters.",
    },
}
