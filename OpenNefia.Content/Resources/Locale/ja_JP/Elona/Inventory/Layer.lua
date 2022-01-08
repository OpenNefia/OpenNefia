Elona.Inventory.Layer =
{
   Topic = {
      ItemName = "アイテムの名称",
      ItemWeight = "重さ"
   },

   Note = {
      TotalWeight = function(totalWeight, maxWeight, cargoWeight)
         return ("重さ合計 %s/%s  荷車 %s")
            :format(totalWeight, maxWeight, cargoWeight)
      end,
   }
}
