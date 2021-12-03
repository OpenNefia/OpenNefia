OpenNefia.Content.Logic.CharaAction =
{
   PickUpItem = function(_1, _2)
      return ("%s pick%s up %s.")
         :format(_.name(_1), _.s(_1), _2)
   end,
   DropItem = function(_1, _2)
      return ("%s drop%s %s.")
         :format(_.name(_1), _.s(_1), _2)
   end,
}
