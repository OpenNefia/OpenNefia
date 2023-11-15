Elona.Inventory.Common = {
    DoesNotExist = "The item doesn't exist.",
    SetAsNoDrop = "It's set as no-drop. You can reset it from the <examine> menu.",
    InventoryIsFull = "Your inventory is full.",

    HowMany = function(min, max, entity)
        return ("How many? (%s to %s)"):format(min, max)
    end,

    Invalid = function(entity, id)
        return ("Invalid Item Id found. Item No:%s, Id:%s has been removed from your inventory."):format(entity, id)
    end,

    SomethingFalls = {
        FromBackpack = function(item, owner)
            return ("%s falls down from %s backpack."):format(_.name(item), _.possessive(owner))
        end,
        AndDisappears = "Something falls on the ground and disappears...",
    },

    NameModifiers = {
        Ground = "(Ground)",
    },
}
