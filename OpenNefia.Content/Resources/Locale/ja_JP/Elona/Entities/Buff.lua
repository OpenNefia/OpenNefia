OpenNefia.Prototypes.Entity.Elona = {
    BuffHolyShield = {
        Buff = {
            Apply = function(_1)
                return ("%sは光り輝いた。"):format(_.name(_1))
            end,
            Description = function(_1)
                return ("PVを%s上昇/耐恐怖"):format(_1)
            end,
        },
        MetaData = {
            Name = "聖なる盾",
        },
    },
    BuffMistOfSilence = {
        Buff = {
            Apply = function(_1)
                return ("%sはぼやけた霧に覆われた。"):format(_.name(_1))
            end,
            Description = "魔法の使用を禁止",
        },
        MetaData = {
            Name = "沈黙の霧",
        },
    },
    BuffRegeneration = {
        Buff = {
            Apply = function(_1)
                return ("%sの代謝が活性化した。"):format(_.name(_1))
            end,
            Description = "自然回復強化",
        },
        MetaData = {
            Name = "リジェネレーション",
        },
    },
    BuffElementalShield = {
        Buff = {
            Apply = function(_1)
                return ("%sは元素への耐性を得た。"):format(_.name(_1))
            end,
            Description = "炎冷気電撃耐性の獲得",
        },
        MetaData = {
            Name = "元素保護",
        },
    },
    BuffSpeed = {
        Buff = {
            Apply = function(_1)
                return ("%sは機敏になった。"):format(_.name(_1))
            end,
            Description = function(_1)
                return ("%sの加速"):format(_1)
            end,
        },
        MetaData = {
            Name = "加速",
        },
    },
    BuffSlow = {
        Buff = {
            Apply = function(_1)
                return ("%sは鈍重になった。"):format(_.name(_1))
            end,
            Description = function(_1)
                return ("%sの鈍足"):format(_1)
            end,
        },
        MetaData = {
            Name = "鈍足",
        },
    },
    BuffHero = {
        Buff = {
            Apply = function(_1)
                return ("%sの士気が向上した。"):format(_.name(_1))
            end,
            Description = function(_1)
                return ("筋力・器用を%s上昇/耐恐怖/耐混乱"):format(_1)
            end,
        },
        MetaData = {
            Name = "英雄",
        },
    },
    BuffMistOfFrailness = {
        Buff = {
            Apply = function(_1)
                return ("%sは脆くなった。"):format(_.name(_1))
            end,
            Description = "DVとPVを半減",
        },
        MetaData = {
            Name = "脆弱の霧",
        },
    },
    BuffElementScar = {
        Buff = {
            Apply = function(_1)
                return ("%sは元素への耐性を失った。"):format(_.name(_1))
            end,
            Description = "炎冷気電撃耐性の減少",
        },
        MetaData = {
            Name = "元素の傷跡",
        },
    },
    BuffHolyVeil = {
        Buff = {
            Apply = function(_1)
                return ("%sは聖なる衣に保護された。"):format(_.name(_1))
            end,
            Description = function(_1)
                return ("ﾊﾟﾜｰ%sの呪い(hex)への抵抗"):format(_1)
            end,
        },
        MetaData = {
            Name = "ホーリーヴェイル",
        },
    },
    BuffNightmare = {
        Buff = {
            Apply = function(_1)
                return ("%sは悪夢に襲われた。"):format(_.name(_1))
            end,
            Description = "神経幻惑耐性の減少",
        },
        MetaData = {
            Name = "ナイトメア",
        },
    },
    BuffDivineWisdom = {
        Buff = {
            Apply = function(_1)
                return ("%sの思考は冴え渡った。"):format(_.name(_1))
            end,
            Description = function(_1, _2)
                return ("習得・魔力を%s上昇/読書を%s上昇"):format(_1, _2)
            end,
        },
        MetaData = {
            Name = "知者の加護",
        },
    },
    BuffPunishment = {
        Buff = {
            Apply = function(_1)
                return ("%sは雷に打たれた！"):format(_.name(_1))
            end,
            Description = function(_1, _2)
                return ("%sの鈍足/PVを%s%%減少"):format(_1, _2)
            end,
        },
        MetaData = {
            Name = "天罰",
        },
    },
    BuffLulwysTrick = {
        Buff = {
            Apply = function(_1)
                return ("%sにルルウィが乗り移った。"):format(_.name(_1))
            end,
            Description = function(_1)
                return ("%sの加速"):format(_1)
            end,
        },
        MetaData = {
            Name = "ルルウィの憑依",
        },
    },
    BuffIncognito = {
        Buff = {
            Apply = function(_1)
                return ("%sは別人になりすました。"):format(_.name(_1))
            end,
            Description = "変装",
        },
        MetaData = {
            Name = "インコグニート",
        },
    },
    BuffDeathWord = {
        Buff = {
            Apply = function(_1)
                return ("%sは死の宣告を受けた！"):format(_.name(_1))
            end,
            Description = "呪いが完了したときに確実なる死",
        },
        MetaData = {
            Name = "死の宣告",
        },
    },
    BuffBoost = {
        Buff = {
            Apply = function(_1)
                return ("%sはブーストした！"):format(_.name(_1))
            end,
            Description = function(_1)
                return ("%sの加速と能力のアップ"):format(_1)
            end,
        },
        MetaData = {
            Name = "ブースト",
        },
    },
    BuffContingency = {
        Buff = {
            Apply = function(_1)
                return ("%sは死神と契約した。"):format(_.name(_1))
            end,
            Description = function(_1)
                return ("致命傷を負ったとき%s%%の確率でダメージ分回復。"):format(_1)
            end,
        },
        MetaData = {
            Name = "契約",
        },
    },
    BuffLucky = {
        Buff = {
            Apply = function(_1)
                return ("%sに幸運な日が訪れた！"):format(_.name(_1))
            end,
            Description = function(_1)
                return ("%sの幸運の上昇"):format(_1)
            end,
        },
        MetaData = {
            Name = "幸運",
        },
    },
    BuffFoodStrength = {
        Buff = {
            Apply = nil,
            Description = function(_1)
                return ("筋力の成長率を%s%%上昇"):format(_1)
            end,
        },
        MetaData = {
            Name = "筋力の成長",
        },
    },
    BuffFoodConstitution = {
        Buff = {
            Apply = nil,
            Description = function(_1)
                return ("耐久の成長率を%s%%上昇"):format(_1)
            end,
        },
        MetaData = {
            Name = "耐久の成長",
        },
    },
    BuffFoodDexterity = {
        Buff = {
            Apply = nil,
            Description = function(_1)
                return ("器用の成長率を%s%%上昇"):format(_1)
            end,
        },
        MetaData = {
            Name = "器用の成長",
        },
    },
    BuffFoodPerception = {
        Buff = {
            Apply = nil,
            Description = function(_1)
                return ("感覚の成長率を%s%%上昇"):format(_1)
            end,
        },
        MetaData = {
            Name = "感覚の成長",
        },
    },
    BuffFoodLearning = {
        Buff = {
            Apply = nil,
            Description = function(_1)
                return ("習得の成長率を%s%%上昇"):format(_1)
            end,
        },
        MetaData = {
            Name = "習得の成長",
        },
    },
    BuffFoodWill = {
        Buff = {
            Apply = nil,
            Description = function(_1)
                return ("意思の成長率を%s%%上昇"):format(_1)
            end,
        },
        MetaData = {
            Name = "意思の成長",
        },
    },
    BuffFoodMagic = {
        Buff = {
            Apply = nil,
            Description = function(_1)
                return ("魔力の成長率を%s%%上昇"):format(_1)
            end,
        },
        MetaData = {
            Name = "魔力の成長",
        },
    },
    BuffFoodCharisma = {
        Buff = {
            Apply = nil,
            Description = function(_1)
                return ("魅力の成長率を%s%%上昇"):format(_1)
            end,
        },
        MetaData = {
            Name = "魅力の成長",
        },
    },
    BuffFoodSpeed = {
        Buff = {
            Apply = nil,
            Description = function(_1)
                return ("速度の成長率を%s%%上昇"):format(_1)
            end,
        },
        MetaData = {
            Name = "速度の成長",
        },
    },
}
