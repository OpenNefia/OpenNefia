Elona.CurseState =
{
   Equipped = {
      Blessed = function(actor, target, item)
         return ("%s feel%s as if someone is watching %s intently.")
            :format(_.name(target), _.s(target), _.him(target))
      end,
      Cursed = function(actor, target, item)
         return ("%s suddenly feel%s a chill and shudder%s.")
            :format(_.name(target), _.s(target), _.s(target))
      end,
      Doomed = function(actor, target, item)
         return ("%s %s now one step closer to doom.")
            :format(_.name(target), _.is(target))
      end,
   }
}
