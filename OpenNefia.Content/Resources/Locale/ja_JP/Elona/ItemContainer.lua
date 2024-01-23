Elona.ItemContainer = {
    Put = {
        Succeed = function(entity, target)
            return ("%s%sを保管した。"):format(_.sore_wa(entity), _.name(target, true))
        end,
        Errors = {
            ContainerIsFull = "これ以上入らない。",
            TotalWeightTooHeavy = function(maxWeight)
                return ("合計重量が%sを超えない。"):format(maxWeight)
            end,
            ItemTooHeavy = function(container, maxWeight)
                return ("重さが%s以上の物は入らない。"):format(maxWeight)
            end,
            CannotHoldCargo = "荷物は入らない。",
        },
    },

    FourDimensionalPocket = {
        TotalWeight = function(currentItems, maxItems, currentWeight, maxWeight, maxItemWeight)
            return ("%s/%s items (重さ合計 %s/%s, 単体重さ上限: %s)"):format(
                currentItems,
                maxItems,
                currentWeight,
                maxWeight,
                maxItemWeight
            )
        end,
    },
}
