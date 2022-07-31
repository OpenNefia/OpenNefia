Elona.Magic = {
    FailToCast = {
        CreaturesAreSummoned = "Several creatures are summoned from a vortex of magic.",
        DimensionDoorOpens = function(chara)
            return ("A dimension door opens in front of %s."):format(_.name(chara))
        end,
        IsConfusedMore = function(chara)
            return ("%s %s confused more."):format(_.name(chara), _.is(chara))
        end,
        TooDifficult = "It's too difficult!",
        ManaIsAbsorbed = function(chara)
            return ("%s mana is absorbed."):format(_.possessive(chara))
        end,
    },
}
