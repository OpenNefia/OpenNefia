OpenNefia.Content.Object.Aspect.Types.PotionAspect =
{
   DrinksPotion = function(_1, _2)
      return ("%s drink%s %s.")
         :format(_.name(_1), _.s(_1), _2)
   end,

   ThrownShatters = "It falls on the ground and shatters.",
   ThrownHits = function(_1)
      return ("It hits %s!")
         :format(_.name(_1))
   end,
}
