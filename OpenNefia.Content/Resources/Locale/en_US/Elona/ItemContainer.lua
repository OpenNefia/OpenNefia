Elona.ItemContainer = {
    Put = {
        Succeed = function(entity, target)
            return ("%s put%s %s into the container."):format(_.name(entity), _.s(entity), _.name(target, true))
        end,
        Errors = {
            ContainerIsFull = "The container is full.",
            TotalWeightTooHeavy = function(maxWeight)
                return ("The container can only hold a total weight of less than %s."):format(maxWeight)
            end,
            ItemTooHeavy = function(maxWeight)
                return ("The container cannot hold items heavier than %s."):format(maxWeight)
            end,
            CannotHoldCargo = "The container cannot hold cargos.",
        },
    },

    FourDimensionalPocket = {
        TotalWeight = function(currentItems, maxItems, currentWeight, maxWeight, maxItemWeight)
            return ("%s/%s items (Weight %s/%s, Max Item Weight: %s)"):format(
                currentItems,
                maxItems,
                currentWeight,
                maxWeight,
                maxItemWeight
            )
        end,
    },
}
