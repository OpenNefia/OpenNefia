Elona.Inventory.Layer =
{
   Topic = {
      ItemName = "Name",
      ItemWeight = "Weight"
   },

   Note = {
      TotalWeight = function(totalWeight, maxWeight, cargoWeight)
         return ("Weight %s/%s  Cargo %s")
            :format(totalWeight, maxWeight, cargoWeight)
      end,
   }
}
