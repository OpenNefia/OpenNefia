OpenNefia.Prototypes.Entity.Elona = {
    BuffHolyShield = {
        Buff = {
            Apply = function(_1)
                return ("%s发出耀眼的光芒。"):format(_.name(_1))
            end,
            Description = function(_1)
                return ("提升%s点生命值/免疫恐惧"):format(_1)
            end,
        },
        MetaData = {
            Name = "圣之盾",
        },
    },
    BuffMistOfSilence = {
        Buff = {
            Apply = function(_1)
                return ("%s被笼罩在迷雾中。"):format(_.name(_1))
            end,
            Description = "禁止使用魔法",
        },
        MetaData = {
            Name = "寂静之雾",
        },
    },
    BuffRegeneration = {
        Buff = {
            Apply = function(_1)
                return ("%s的新陈代谢被激活。"):format(_.name(_1))
            end,
            Description = "强化自然恢复",
        },
        MetaData = {
            Name = "再生",
        },
    },
    BuffElementalShield = {
        Buff = {
            Apply = function(_1)
                return ("%s获得了元素抗性。"):format(_.name(_1))
            end,
            Description = "获得火焰、寒冷和闪电抗性",
        },
        MetaData = {
            Name = "元素护盾",
        },
    },
    BuffSpeed = {
        Buff = {
            Apply = function(_1)
                return ("%s变得敏捷了。"):format(_.name(_1))
            end,
            Description = function(_1)
                return ("加速%s点"):format(_1)
            end,
        },
        MetaData = {
            Name = "加速",
        },
    },
    BuffSlow = {
        Buff = {
            Apply = function(_1)
                return ("%s变得迟钝了。"):format(_.name(_1))
            end,
            Description = function(_1)
                return ("减速%s点"):format(_1)
            end,
        },
        MetaData = {
            Name = "减速",
        },
    },
    BuffHero = {
        Buff = {
            Apply = function(_1)
                return ("%s的士气提升了。"):format(_.name(_1))
            end,
            Description = function(_1)
                return ("力量和灵巧提升%s点/免疫恐惧和混乱"):format(_1)
            end,
        },
        MetaData = {
            Name = "英雄",
        },
    },
    BuffMistOfFrailness = {
        Buff = {
            Apply = function(_1)
                return ("%s变得脆弱了。"):format(_.name(_1))
            end,
            Description = "DV和PV减半",
        },
        MetaData = {
            Name = "脆弱之雾",
        },
    },
    BuffElementScar = {
        Buff = {
            Apply = function(_1)
                return ("%s失去了对元素的抗性。"):format(_.name(_1))
            end,
            Description = "火焰、寒冷和闪电抗性减少",
        },
        MetaData = {
            Name = "元素伤痕",
        },
    },
    BuffHolyVeil = {
        Buff = {
            Apply = function(_1)
                return ("%s受到了圣衣的保护。"):format(_.name(_1))
            end,
            Description = function(_1)
                return ("对%s的诅咒(hex)具有抵抗力"):format(_1)
            end,
        },
        MetaData = {
            Name = "神圣面纱",
        },
    },
    BuffNightmare = {
        Buff = {
            Apply = function(_1)
                return ("%s遭受了噩梦的袭击。"):format(_.name(_1))
            end,
            Description = "神经幻惑抵抗力减少",
        },
        MetaData = {
            Name = "梦魇",
        },
    },
    BuffDivineWisdom = {
        Buff = {
            Apply = function(_1)
                return ("%s的思维变得敏锐起来。"):format(_.name(_1))
            end,
            Description = function(_1, _2)
                return ("学识和魔力提升%s点/阅读能力提升%s点"):format(_1, _2)
            end,
        },
        MetaData = {
            Name = "知者的庇护",
        },
    },
    BuffPunishment = {
        Buff = {
            Apply = function(_1)
                return ("%s遭受雷击！"):format(_.name(_1))
            end,
            Description = function(_1, _2)
                return ("减速%s点/PV减少%s%%"):format(_1, _2)
            end,
        },
        MetaData = {
            Name = "天罚",
        },
    },
    BuffLulwysTrick = {
        Buff = {
            Apply = function(_1)
                return ("%s受到卢尔维的附体。"):format(_.name(_1))
            end,
            Description = function(_1)
                return ("加速%s点"):format(_1)
            end,
        },
        MetaData = {
            Name = "卢尔维的附体",
        },
    },
    BuffIncognito = {
        Buff = {
            Apply = function(_1)
                return ("%s变成了另一个人。"):format(_.name(_1))
            end,
            Description = "变装",
        },
        MetaData = {
            Name = "匿名化",
        },
    },
    BuffDeathWord = {
        Buff = {
            Apply = function(_1)
                return ("%s受到了死亡宣告！"):format(_.name(_1))
            end,
            Description = "确定死亡",
        },
        MetaData = {
            Name = "死亡宣告",
        },
    },
    BuffBoost = {
        Buff = {
            Apply = function(_1)
                return ("%s进行了助推！"):format(_.name(_1))
            end,
            Description = function(_1)
                return ("加速并提高%s的能力"):format(_1)
            end,
        },
        MetaData = {
            Name = "助推",
        },
    },
    BuffContingency = {
        Buff = {
            Apply = function(_1)
                return ("%s和死神签订了契约。"):format(_.name(_1))
            end,
            Description = function(_1)
                return ("受致命伤时有%s%%几率回复受到的伤害。"):format(_1)
            end,
        },
        MetaData = {
            Name = "契约",
        },
    },
    BuffLucky = {
        Buff = {
            Apply = function(_1)
                return ("%s迎来了幸运的一天！"):format(_.name(_1))
            end,
            Description = function(_1)
                return ("幸运提升%s点"):format(_1)
            end,
        },
        MetaData = {
            Name = "幸运",
        },
    },
    BuffFoodStrength = {
        Buff = {
            Apply = nil,
            Description = function(_1)
                return ("提升%s%%力量成长率"):format(_1)
            end,
        },
        MetaData = {
            Name = "力量成长",
        },
    },
    BuffFoodConstitution = {
        Buff = {
            Apply = nil,
            Description = function(_1)
                return ("提升%s%%耐久成长率"):format(_1)
            end,
        },
        MetaData = {
            Name = "耐久成长",
        },
    },
    BuffFoodDexterity = {
        Buff = {
            Apply = nil,
            Description = function(_1)
                return ("提升%s%%器用成长率"):format(_1)
            end,
        },
        MetaData = {
            Name = "器用成长",
        },
    },
    BuffFoodPerception = {
        Buff = {
            Apply = nil,
            Description = function(_1)
                return ("提升%s%%感知成长率"):format(_1)
            end,
        },
        MetaData = {
            Name = "感知成长",
        },
    },
    BuffFoodLearning = {
        Buff = {
            Apply = nil,
            Description = function(_1)
                return ("提升%s%%学识成长率"):format(_1)
            end,
        },
        MetaData = {
            Name = "学识成长",
        },
    },
    BuffFoodWill = {
        Buff = {
            Apply = nil,
            Description = function(_1)
                return ("提升%s%%意志成长率"):format(_1)
            end,
        },
        MetaData = {
            Name = "意志成长",
        },
    },
    BuffFoodMagic = {
        Buff = {
            Apply = nil,
            Description = function(_1)
                return ("提升%s%%魔法成长率"):format(_1)
            end,
        },
        MetaData = {
            Name = "魔法成长",
        },
    },
    BuffFoodCharisma = {
        Buff = {
            Apply = nil,
            Description = function(_1)
                return ("提升%s%%魅力成长率"):format(_1)
            end,
        },
        MetaData = {
            Name = "魅力成长",
        },
    },
    BuffFoodSpeed = {
        Buff = {
            Apply = nil,
            Description = function(_1)
                return ("提升%s%%速度成长率"):format(_1)
            end,
        },
        MetaData = {
            Name = "速度成长",
        },
    },
}