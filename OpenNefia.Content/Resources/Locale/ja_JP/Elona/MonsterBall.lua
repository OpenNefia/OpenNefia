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
        CannotCaptureHere = "この場所では捕獲できない。",
        CannotBeCaptured = "その生物には無効だ。",
        NotEnoughPower = "その生物を捕獲するには、より高レベルのモンスターボールが必要だ。",
        NotWeakEnough = "捕獲するためにはもっと弱らせる必要がある。",
    },

    Throw = {
        YouCapture = function(user, target)
            return ("%s%sをモンスターボールに捕獲した。"):format(_.sore_wa(user), _.name(target))
        end,
    },

    Use = {
        Empty = "モンスターボールは空っぽだ。",
        PartyIsFull = "仲間はこれ以上増やせない。",
        YouUse = function(user, monsterBall)
            return ("%s%sを使用した。"):format(_.sore_wa(user), _.name(monsterBall))
        end,
    },
}
