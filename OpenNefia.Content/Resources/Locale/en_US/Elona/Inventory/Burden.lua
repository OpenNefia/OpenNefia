Elona.Inventory.Burden = {
    CarryTooMuch = "You carry too much to move!",
    BackpackSquashing = function(entity)
        return ("%s%s backpack is squashing %s!"):format(_.name(entity), _.possessive(entity), _.him(entity))
    end,
    Indicator = {
        None = "",
        Light = "Burden",
        Moderate = "Burden!",
        Heavy = "Overweight",
        Max = "Overweight!",
    },
}
