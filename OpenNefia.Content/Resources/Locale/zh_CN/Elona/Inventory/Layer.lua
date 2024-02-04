Elona.Inventory.Layer = {
    Topic = {
        ItemName = "物品名称",
        ItemWeight = "重量",
    },

    Note = {
        TotalWeight = function(totalWeight, maxWeight, cargoWeight, maxCargoWeight)
            return ("总重量 %s/%s  货物重量 %s/%s"):format(totalWeight, maxWeight, cargoWeight, maxCargoWeight)
        end,
    },
}