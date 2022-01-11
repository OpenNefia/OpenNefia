Elona.Inventory.Layer = {
    Topic = {
        ItemName = "Name",
        ItemWeight = "Weight",
    },

    Note = {
        TotalWeight = function(totalWeight, maxWeight, cargoWeight, maxCargoWeight)
            return ("Weight %s/%s  Cargo %s/%s"):format(totalWeight, maxWeight, cargoWeight, maxCargoWeight)
        end,
    },
}
