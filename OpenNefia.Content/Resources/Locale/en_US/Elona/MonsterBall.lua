Elona.MonsterBall = {
    ItemName = {
        Empty = function(name, lv)
            return ("%s Level %s(Empty)"):format(name, lv)
        end,
        Full = function(name, charaName)
            return ("%s(%s)"):format(name, charaName)
        end,
    },

    Throw = {
        DoesNotWork = "This doesn't work in this area.",
        CannotBeCaptured = "The creature can't be captured.",
        NotEnoughPower = "Power level of the ball is not enough to capture the creature.",
        NotWeakEnough = "You need to weaken the creature to capture it.",
        YouCapture = function(user, target)
            return ("%s capture%s %s."):format(_.name(user), _.s(user), name(target))
        end,
    },

    Use = {
        Empty = "This ball is empty.",
        PartyIsFull = "Your party is full.",
        YouUse = function(user, monsterBall)
            return ("%s activate%s %s."):format(_.name(user), _.s(user), _.name(monsterBall))
        end,
    },
}
