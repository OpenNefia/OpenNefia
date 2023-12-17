Elona.Magic = {
    FailToCast = {
        CreaturesAreSummoned = "魔力の渦が何かを召喚した！",
        DimensionDoorOpens = function(chara)
            return ("%sは奇妙な力に捻じ曲げられた！"):format(_.name(chara))
        end,
        IsConfusedMore = function(chara)
            return ("%sは余計に混乱した。"):format(_.name(chara))
        end,
        TooDifficult = "難解だ！",
        ManaIsAbsorbed = function(chara)
            return ("%sはマナを吸い取られた！"):format(_.name(chara))
        end,
    },

    ControlMagic = {
        PassesThrough = function(target)
            return ("%sは巻き込みを免れた。"):format(_.name(target))
        end,
    },
}
