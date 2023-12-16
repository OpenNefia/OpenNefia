Elona.GameObjects.Pickable = {
    NotOwned = {
        "It's not your property.",
        "You can't just take it.",
        "It's not yours.",
    },
    CannotCarry = "You can't carry it.",
    GraspAtAir = "You grasp at the air.",
    IsBeingUsed = function(item, user)
        return ("%s %s using it."):format(_.name(user), _.is(user))
    end,

    PicksUp = function(entity, target)
        return ("%s pick%s up %s."):format(_.name(entity), _.s(entity), _.name(target))
    end,
    Drops = function(entity, target)
        return ("%s drop%s %s."):format(_.name(entity), _.s(entity), _.name(target))
    end,
}
