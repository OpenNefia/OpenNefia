Elona.MonsterBall = {
    ItemName = {
        Empty = function(name, lv)
            return ("%s Lv%s (空)"):format(name, lv)
        end,
        Full = function(name, charaName)
            return ("%s(%s)"):format(name, charaName)
        end,
    },

    Errors = {
        CannotCaptureHere = "无法在这个地方捕捉。",
        CannotBeCaptured = "无法捕捉该生物。",
        NotEnoughPower = "需要更高等级的怪物球才能捕捉该生物。",
        NotWeakEnough = "需要更多削弱该生物的能力。",
    },

    Throw = {
        YouCapture = function(user, target)
            return ("%s捕获了%s。"):format(_.sore_wa(user), _.name(target))
        end,
    },

    Use = {
        Empty = "怪物球是空的。",
        PartyIsFull = "无法再增加成员。",
        YouUse = function(user, monsterBall)
            return ("%s使用了%s。"):format(_.sore_wa(user), _.name(monsterBall))
        end,
    },
}