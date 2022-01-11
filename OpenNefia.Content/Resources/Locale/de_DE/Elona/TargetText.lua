Elona.TargetText =
{
   ItemOnCell = {
      And = " und ",

      MoreThanThree = function(itemCount)
         return ("Hier liegen %s Gegenstände."):format(itemCount)
      end,

      Item = function(itemNames)
         return ("Du siehst hier %s."):format(itemNames)
      end,
      Construct = function(itemNames)
         return ("%s ist hier eingerichtet."):format(itemNames)
      end,
      NotOwned = function(itemNames)
         return ("Du siehst hier %s plaziert."):format(itemNames)
      end,
   }
}
