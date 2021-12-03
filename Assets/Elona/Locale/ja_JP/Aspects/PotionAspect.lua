OpenNefia.Core.Object.Aspect.Types.PotionAspect =
{
   DrinksPotion = function(_1, _2)
      return ("%s%sを飲み干した。")
         :format(_.kare_wa(_1), _2)
   end,

   ThrownShatters = "それは地面に落ちて砕けた。",
   ThrownHits = function(_1)
      return ("%sに見事に命中した！")
         :format(_.name(_1))
   end,
}
