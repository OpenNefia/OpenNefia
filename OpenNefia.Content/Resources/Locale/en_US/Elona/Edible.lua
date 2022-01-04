Elona.Edible =
{
   Eats = function(entity, edible)
      return ("%s eat%s %s.")
         :format(_.name(entity), _.s(entity), _.name(edible))
   end,
}
