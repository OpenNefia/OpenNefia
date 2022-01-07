Elona.EquipSlots =
{
    CannotEquip = function(actor, target, item)
       return ("%s can't equip %s.")
          :format(_.name(actor), _.name(item))
    end,
    CannotUnequip = function(actor, target, item)
       return ("%s can't unequip %s.")
          :format(_.name(actor), _.name(item))
    end
}
