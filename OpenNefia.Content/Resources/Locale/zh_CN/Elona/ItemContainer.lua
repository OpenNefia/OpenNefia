Elona.ItemContainer = {
    Put = {
        Succeed = function(entity, target)
            return ("%s%s已被储存。"):format(_.sore_wa(entity), _.name(target, true))
        end,
        Errors = {
            ContainerIsFull = "无法再装入。",
            TotalWeightTooHeavy = function(maxWeight)
                return ("总重量不能超过%s。"):format(maxWeight)
            end,
            ItemTooHeavy = function(container, maxWeight)
                return ("无法装入重量超过%s的物品。"):format(maxWeight)
            end,
            CannotHoldCargo = "无法装入货物。",
        },
    },

    FourDimensionalPocket = {
        TotalWeight = function(currentItems, maxItems, currentWeight, maxWeight, maxItemWeight)
            return ("%s/%s个物品 (总重量 %s/%s, 单个物品重量上限: %s)"):format(
                currentItems,
                maxItems,
                currentWeight,
                maxWeight,
                maxItemWeight
            )
        end,
    },
}