Elona.GameObjects.Pickable =
{
   NotOwned = { "それはあなたの物ではない。", "盗むなんてとんでもない。", "それは拾えない。" },
   CannotCarry = "それは持ち運べない。",
   GraspAtAir = "あなたは空気をつかんだ。",

   PicksUp = function(entity, target)
      return ("%sは%sを拾った。")
         :format(_.name(entity), target)
   end,
   Drops = function(_entity, target)
      return ("%sを地面に置いた。")
         :format(target)
   end,
}
