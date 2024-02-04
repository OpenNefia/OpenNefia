OpenNefia.Prototypes.Elona.Skill.Elona = {
    AttrStrength = {
        Name = "筋力",
        ShortName = "筋力",
        OnDecrease = function(entity)
            return ("%s感觉自己的肥肉似乎增多了。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%s变得更强了。"):format(_.name(entity))
        end,
    },
    AttrConstitution = {
        Name = "耐久",
        ShortName = "耐久",
        OnDecrease = function(entity)
            return ("%s变得无法忍耐。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%s开始体会到忍耐的快感。"):format(_.name(entity))
        end,
    },
    AttrDexterity = {
        Name = "器用",
        ShortName = "器用",
        OnDecrease = function(entity)
            return ("%s变得不灵活。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%s变得更加灵巧。"):format(_.name(entity))
        end,
    },
    AttrPerception = {
        Name = "感觉",
        ShortName = "感觉",
        OnDecrease = function(entity)
            return ("%s感到感觉有些偏差。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%s开始感受到世界更加亲近。"):format(_.name(entity))
        end,
    },
    AttrLearning = {
        Name = "学习",
        ShortName = "学习",
        OnDecrease = function(entity)
            return ("%s的学习欲望下降了。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%s突然想要学习各种事情。"):format(_.name(entity))
        end,
    },
    AttrWill = {
        Name = "意志",
        ShortName = "意志",
        OnDecrease = function(entity)
            return ("%s什么都马上放弃了。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%s的意志变得坚定了。"):format(_.name(entity))
        end,
    },
    AttrMagic = {
        Name = "魔力",
        ShortName = "魔力",
        OnDecrease = function(entity)
            return ("%s感觉到魔力的衰退。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%s感觉到魔力的增强。"):format(_.name(entity))
        end,
    },
    AttrCharisma = {
        Name = "魅力",
        ShortName = "魅力",
        OnDecrease = function(entity)
            return ("%s突然讨厌被人看见。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%s感到周围人的目光舒适。"):format(_.name(entity))
        end,
    },
    AttrSpeed = {
        Name = "速度",
        OnDecrease = function(entity)
            return ("%s变慢了。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%s感觉周围的动作变得慢了。"):format(_.name(entity))
        end,
    },
    AttrLuck = {
        Name = "运势",
        OnDecrease = function(entity)
            return ("%s变得不幸。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%s变得幸运。"):format(_.name(entity))
        end,
    },
    AttrLife = {
        Name = "生命力",
        OnDecrease = function(entity)
            return ("%s感觉到生命力的衰退。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%s感觉到生命力的增强。"):format(_.name(entity))
        end,
    },
    AttrMana = {
        Name = "魔力",
        OnDecrease = function(entity)
            return ("%s感觉到魔力的衰退。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%s感觉到魔力的提升。"):format(_.name(entity))
        end,
    },
}