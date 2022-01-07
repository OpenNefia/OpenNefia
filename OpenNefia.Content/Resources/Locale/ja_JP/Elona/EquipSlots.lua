Elona.EquipSlots =
{
    CannotEquip = function(actor, target, item)
       return ("%sは%sを装備できない。")
          :format(_.name(actor), _.name(item))
    end,
    CannotUnequip = function(actor, target, item)
       return ("%sは%sを外せない。")
          :format(_.name(actor), _.name(item))
    end
}
