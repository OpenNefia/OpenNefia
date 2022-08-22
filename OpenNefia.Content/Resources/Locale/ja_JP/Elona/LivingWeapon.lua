Elona.LivingWeapon = {
    ItemDescription = {
        ItIsAlive = function(item, level, levelBuffed, exp, expToNext)
            local levelStr = tostring(level)
            if levelBuffed ~= level then
                levelStr = levelStr .. "(" .. levelBuffed .. ")"
            end
            local expPercent = math.floor(math.clamp(exp * 100 / expToNext, 0, 100))
            return ("%s生きている [Lv:%s Exp:%s%%]"):format(_.kare_wa(item), levelStr, expPercent)
        end,
    },
    HasTastedEnoughBlood = function(item)
        return ("%sは十分に血を味わった！"):format(_.name(item))
    end,

    Grow = {
        NeedsMoreBlood = "この武器はまだ血を吸い足りない。",
        ReadyToGrow = function(item)
            return ("%sは十分に血を吸い成長できる！"):format(_.name(item))
        end,
        ButYouSenseSomethingWeird = "しかし、なんだか様子がおかしい…",
        It = "それは…",

        Choices = {
            AddBonus = "ボーナス+1",
        },

        VibratesDispleased = function(item)
            return ("%sは不満そうに震えた。"):format(_.name(item))
        end,

        VibratesPleased = function(item)
            return ("%sは嬉しげに震えた。"):format(_.name(item))
        end,

        ItsPowerIsBecomingAThreat = "その力は次第に脅威になっている。",
        RemovesAnEnchantment = function(item)
            ("%sはエンチャントを消した。"):format(_.name(item))
        end,
    },
}
