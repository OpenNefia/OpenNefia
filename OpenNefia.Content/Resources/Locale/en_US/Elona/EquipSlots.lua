Elona.EquipSlots = {
    Equip = {
        Succeeds = function(actor, target, item)
            return ("%s equip%s %s."):format(_.name(target), _.s(target), _.name(item))
        end,
        Fails = function(actor, target, item)
            return ("%s can't be equipped."):format(_.name(item))
        end,
    },
    Unequip = {
        Succeeds = function(actor, target, item)
            return ("%s unequip%s %s."):format(_.name(target), _.s(target), _.name(item))
        end,
        Fails = function(actor, target, item)
            return ("%s can't be taken off."):format(_.name(item))
        end,
    },
}
