Elona.GameObjects.Pickable = {
    NotOwned = {
        "それはあなたの物ではない。",
        "盗むなんてとんでもない。",
        "それは拾えない。",
    },
    CannotCarry = "それは持ち運べない。",
    GraspAtAir = "あなたは空気をつかんだ。",
    IsBeingUsed = function(item, user)
        return ("それは%sが使用中だ。"):format(_.name(user))
    end,

    PicksUp = function(entity, target)
        return ("%sは%sを拾った。"):format(_.name(entity), _.name(target))
    end,
    Drops = function(_entity, target)
        return ("%sを地面に置いた。"):format(_.name(target))
    end,
}
