Elona.Edible =
{
   Starts = function(entity, edible)
      return ("%s start%s to eat %s.")
         :format(_.name(entity), _.s(entity), _.name(edible))
   end,
   Finishes = function(entity, edible)
      return ("%s %s finished eating %s.")
         :format(_.name(entity), _.have(entity), _.name(edible))
   end,
}
