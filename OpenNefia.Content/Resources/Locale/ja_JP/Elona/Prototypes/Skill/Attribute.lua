OpenNefia.Prototypes.Elona.Skill.Elona = {
    AttrStrength = {
        Name = "筋力",
        ShortName = "筋力",
        OnDecrease = function(entity)
            return ("%sは少し贅肉が増えたような気がした。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sはより強くなった。"):format(_.name(entity))
        end,
    },
    AttrConstitution = {
        Name = "耐久",
        ShortName = "耐久",
        OnDecrease = function(entity)
            return ("%sは我慢ができなくなった。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは我慢することの快感を知った。"):format(_.name(entity))
        end,
    },
    AttrDexterity = {
        Name = "器用",
        ShortName = "器用",
        OnDecrease = function(entity)
            return ("%sは不器用になった。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは器用になった。"):format(_.name(entity))
        end,
    },
    AttrPerception = {
        Name = "感覚",
        ShortName = "感覚",
        OnDecrease = function(entity)
            return ("%sは感覚のずれを感じた。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは世界をより身近に感じるようになった。"):format(_.name(entity))
        end,
    },
    AttrLearning = {
        Name = "習得",
        ShortName = "習得",
        OnDecrease = function(entity)
            return ("%sの学習意欲が低下した。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは急に色々なことを学びたくなった。"):format(_.name(entity))
        end,
    },
    AttrWill = {
        Name = "意思",
        ShortName = "意思",
        OnDecrease = function(entity)
            return ("%sは何でもすぐ諦める。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sの意思は固くなった。"):format(_.name(entity))
        end,
    },
    AttrMagic = {
        Name = "魔力",
        ShortName = "魔力",
        OnDecrease = function(entity)
            return ("%sは魔力の衰えを感じた。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは魔力の上昇を感じた。"):format(_.name(entity))
        end,
    },
    AttrCharisma = {
        Name = "魅力",
        ShortName = "魅力",
        OnDecrease = function(entity)
            return ("%sは急に人前に出るのが嫌になった。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは周囲の視線を心地よく感じる。"):format(_.name(entity))
        end,
    },
    AttrSpeed = {
        Name = "速度",
        OnDecrease = function(entity)
            return ("%sは遅くなった。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは周りの動きが遅く見えるようになった。"):format(_.name(entity))
        end,
    },
    AttrLuck = {
        Name = "運勢",
        OnDecrease = function(entity)
            return ("%sは不幸になった。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは幸運になった。"):format(_.name(entity))
        end,
    },
    AttrLife = {
        Name = "生命力",
        ShortName = "生命力",
        OnDecrease = function(entity)
            return ("%sは生命力の衰えを感じた。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは生命力の上昇を感じた。"):format(_.name(entity))
        end,
    },
    AttrMana = {
        Name = "マナ",
        ShortName = "マナ",
        OnDecrease = function(entity)
            return ("%sはマナの衰えを感じた。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sはマナの向上を感じた。"):format(_.name(entity))
        end,
    },
}
