Elona.Door =
{
   Open = {
      Succeeds = function(entity)
         return ("%s open%s the door.")
            :format(_.name(entity), _.s(entity))
      end,
      Fails = function(entity)
         return ("%s fail%s to open the door.")
            :format(_.name(entity), _.s(entity))
      end,
   }
}
