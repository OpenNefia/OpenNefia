OpenNefia.Content.Logic.CharaAction =
{
   PickUpItem = function(_1, _2)
      return ("%sは%sを拾った。")
         :format(_.name(_1), _2)
   end,
   DropItem = function(_1, _2)
      return ("%sを地面に置いた。")
         :format(_2)
   end,
}
