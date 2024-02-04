Elona.EquipSlots = {
    Equip = {
        Succeeds = function(actor, target, item)
            return ("成功装备了%s。"):format(_.name(item))
        end,
        Fails = function(actor, target, item)
            return ("无法装备%s。"):format(_.name(item))
        end,
    },
    Unequip = {
        Succeeds = function(actor, target, item)
            return ("成功卸下了%s。"):format(_.name(item))
        end,
        Fails = function(actor, target, item)
            return ("无法卸下%s。"):format(_.name(item))
        end,
    },
}