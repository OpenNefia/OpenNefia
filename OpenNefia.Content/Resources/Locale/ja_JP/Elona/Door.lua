Elona.Door =
{
   Open = {
      Succeeds = function(entity)
         return ("%sは扉を開けた。")
            :format(_.name(entity))
      end,
      Fails = function(entity)
         return ("%s開錠に失敗した。")
            :format(_.kare_wa(entity))
      end,
   }
}
