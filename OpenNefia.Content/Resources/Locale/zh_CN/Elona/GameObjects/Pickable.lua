Elona.GameObjects.Pickable = {
    NotOwned = {
        "这不是你的物品。",
        "不要偷东西。",
        "无法捡起该物品。",
    },
    CannotCarry = "无法携带该物品。",
    GraspAtAir = "你抓了一把空气。",
    IsBeingUsed = function(item, user)
        return ("%s正在使用该物品。"):format(_.name(user))
    end,

    PicksUp = function(entity, target)
        return ("%s捡起了%s。"):format(_.name(entity), _.name(target))
    end,
    Drops = function(_entity, target)
        return ("%s放置在地上。"):format(_.name(target))
    end,
}