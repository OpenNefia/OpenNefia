Elona.PlayerMovement =
{
   SenseSomething = "You sense something under your foot.",

   PromptLeaveMap = function(map)
      return ("Do you want to leave %s? ")
         :format(_.name(map))
   end
}
