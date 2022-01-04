Elona.Edible =
{
   Starts = function(entity, edible)
      return ("%sは%sを口に運んだ。")
         :format(_.name(entity), _.name(edible))
   end,
   Finishes = function(entity, edible)
      return ("%s%sを食べ終えた。")
        :format(_.name(entity), _.name(edible))
   end,

}
