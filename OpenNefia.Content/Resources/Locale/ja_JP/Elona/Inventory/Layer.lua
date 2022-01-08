Elona.Inventory.Layer =
{
   Topic = {
      ItemName = "アイテムの名称",
      ItemWeight = "重さ"
   },

   Note = {
      TotalWeight = function(totalWeight, maxWeight, cargoWeight, maxCargoWeight)
         return ("重さ合計 %s/%s  荷車 %s/%s")
            :format(totalWeight, maxWeight, cargoWeight, maxCargoWeight)
      end,
   }
}
