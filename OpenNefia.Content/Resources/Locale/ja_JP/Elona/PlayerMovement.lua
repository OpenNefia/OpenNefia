Elona.PlayerMovement =
{
   SenseSomething = "地面に何かがあるようだ。",

   PromptLeaveMap = function(map)
      return ("%sを去る？ ")
         :format(_.name(map))
   end
}
