Elona.Drinkable = {
    Drinks = function(entity, item)
        return ("%s drink%s %s."):format(_.name(entity), _.s(entity), _.name(item))
    end,
    Thrown = {
        Shatters = "It falls on the ground and shatters.",
    },
}
