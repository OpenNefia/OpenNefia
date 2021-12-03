OpenNefia.Content.Logic.TargetText =
{
   OutOfSight = "This location is out of sight.",
   YouAreTargeting = function(_1, _2)
      return ("You are targeting %s.(Distance %s)")
         :format(_.name(_1), _2)
   end,

   TargetLevel = {
      [0] = function(_1)
         return ("You can absolutely beat %s with your eyes closed and arms crossed.")
            :format(_.him(_1))
      end,
      [1] = function(_1)
         return ("You bet you can beat %s with your eyes closed.")
            :format(_.him(_1))
      end,
      [2] = function(_1)
         return ("%s %s an easy opponent.")
            :format(_.he(_1), _.is(_1))
      end,
      [3] = "You will probably win.",
      [4] = "Won't be an easy fight.",
      [5] = "The opponent looks quite strong.",
      [6] = function(_1)
         return ("%s %s at least twice stronger than you.")
            :format(_.he(_1), _.is(_1))
      end,
      [7] = "You will get killed unless miracles happen.",
      [8] = "You will get killed, a hundred percent sure.",
      [9] = function(_1)
         return ("%s can mince you with %s eyes closed.")
            :format(_.he(_1), _.his(_1))
      end,
      [10] = function(_1)
         return ("If %s is a giant, you are less than a dropping of an ant.")
            :format(_.he(_1))
      end
   }
}
