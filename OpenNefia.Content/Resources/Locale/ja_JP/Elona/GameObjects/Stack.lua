Elona.GameObjects.Stack =
{
   HasBeenStacked = function(entity, totalCount)
      return ("%sをまとめた(計%s個) ")
         :format(entity, totalCount)
   end,
}
