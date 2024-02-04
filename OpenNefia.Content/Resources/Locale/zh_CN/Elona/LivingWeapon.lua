Elona.LivingWeapon = {
    ItemDescription = {
        ItIsAlive = function(item, level, levelBuffed, exp, expToNext)
            local levelStr = tostring(level)
            if levelBuffed ~= level then
                levelStr = levelStr .. "(" .. levelBuffed .. ")"
            end
            local expPercent = math.floor(math.clamp(exp * 100 / expToNext, 0, 100))
            return ("%s活着着呢 [等级:%s 经验:%s%%]"):format(_.kare_wa(item), levelStr, expPercent)
        end,
    },
    HasTastedEnoughBlood = function(item)
        return ("%s已经喝足了血！"):format(_.name(item))
    end,

    Grow = {
        NeedsMoreBlood = "这个武器还没有吸足血。",
        ReadyToGrow = function(item)
            return ("%s已经吸足了血可以成长了！"):format(_.name(item))
        end,
        ButYouSenseSomethingWeird = "但你感觉有些奇怪...",
        It = "它是...",

        Choices = {
            AddBonus = "加上奖励+1",
        },

        VibratesDispleased = function(item)
            return ("%s不满地颤抖着。"):format(_.name(item))
        end,

        VibratesPleased = function(item)
            return ("%s欢喜地颤抖着。"):format(_.name(item))
        end,

        ItsPowerIsBecomingAThreat = "它的力量渐渐变得威胁性起来。",
        RemovesAnEnchantment = function(item)
            return ("%s消除了一个附魔。"):format(_.name(item))
        end,
    },
}