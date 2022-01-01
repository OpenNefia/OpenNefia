Elona.Door =
{
   QueryClose = "Which door do you want to close?",
   Open = {
      Succeeds = function(entity)
         return ("%s open%s the door.")
            :format(_.name(entity), _.s(entity))
      end,
      Fails = function(entity)
         return ("%s fail%s to open the door.")
            :format(_.name(entity), _.s(entity))
      end,
   },
   Close = {
      Succeeds = function(entity)
         return ("%s close%s the door.")
            :format(_.name(entity), _.s(entity))
      end,
      Blocked = "There's something on the floor.",
      NothingToClose = "There's nothing to close."
   }
}
