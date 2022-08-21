Elona.LivingWeapon = {
    ItemDescription = {
        ItIsAlive = function(item, level, levelBuffed, exp, expToNext)
            local levelStr = tostring(level)
            if levelBuffed ~= level then
                levelStr = levelStr .. "(" .. levelBuffed .. ")"
            end
            local expPercent = math.floor(math.clamp(exp * 100 / expToNext, 0, 100))
            return ("%s is alive. [Lv:%s Exp:%s%%]"):format(_.he(item), levelStr, expPercent)
        end,
    },
    HasTastedEnoughBlood = function(item)
        return ("%s has tasted enough blood!"):format(_.name(item))
    end,

    Grow = {
        NeedsMoreBlood = "The weapon needs more blood.",
        ReadyToGrow = function(item)
            return ("%s sucked enough blood and ready to grow!"):format(_.name(item))
        end,
        ButYouSenseSomethingWeird = "But you sense something weird.",
        It = "It...",

        Choices = {
            AddBonus = "Bonus+1",
        },

        VibratesDispleased = function(item)
            return ("%s vibrates as if %s is displeased."):format(_.name(item), _.he(item))
        end,

        VibratesPleased = function(item)
            return ("%s vibrates as if %s is pleased."):format(_.name(item), _.he(item))
        end,

        ItsPowerIsBecomingAThreat = "Its power is becoming a threat.",
        RemovesAnEnchantment = function(item)
            ("%s removes an enchantment."):format(_.name(item))
        end,
    },
}
