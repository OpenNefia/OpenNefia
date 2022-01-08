Elona.EquipSlots =
{
   Equip = {
      Succeeded = function(actor, target, item)
         return ("%sを装備した。")
            :format(_.name(item))
      end,
      Failed = function(actor, target, item)
         return ("%sは装備できない。")
            :format(_.name(item))
      end,
   },
   Unequip = {
      Succeeded = function(actor, target, item)
         return ("%sを外した。")
            :format(_.name(item))
      end,
      Failed = function(actor, target, item)
         return ("%sは外せない。")
            :format(_.name(item))
      end
   }
}
