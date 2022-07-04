Elona.DamageType = {
    Default = {
        Damage = function(entity)
            return ("%sは傷ついた。"):format(_.name(entity))
        end,
        Death = {
            Active = "殺した。",
            Passive = function(entity)
                return ("%sは死んだ。"):format(_.name(entity))
            end,
        },
    },
    Chara = {
        DeathCause = function(entity)
            return ("%sに殺された。"):format(_.basename(entity))
        end,
        Killed = {
            Active = "殺した。",
            Passive = function(entity)
                return ("%sは殺された。"):format(_.name(entity))
            end,
        },
        Minced = {
            Active = "ミンチにした。",
            Passive = function(entity)
                return ("%sはミンチにされた。"):format(_.name(entity))
            end,
        },
        TransformedIntoMeat = {
            Active = "粉々の肉片に変えた。",
            Passive = function(entity)
                return ("%sは粉々の肉片に変えられた。"):format(_.name(entity))
            end,
        },
        Destroyed = {
            Active = "破壊した。",
            Passive = function(entity)
                return ("%sは破壊された。"):format(_.name(entity))
            end,
        },
    },
    Trap = {
        DeathCause = "罠にかかって死んだ。",
        Message = function(entity)
            return ("%sは罠にかかって死んだ。"):format(_.name(entity))
        end,
    },
    MagicReaction = {
        DeathCause = "マナの反動で消滅した。",
        Message = function(entity)
            return ("%sはマナの反動で死んだ。"):format(_.name(entity))
        end,
    },
    Starvation = {
        DeathCause = "飢え死にした。",
        Message = function(entity)
            return ("%sは餓死した。"):format(_.name(entity))
        end,
    },
    Poison = {
        DeathCause = "毒にもがき苦しみながら死んだ。",
        Message = function(entity)
            return ("%sは毒に蝕まれ死んだ。"):format(_.name(entity))
        end,
    },
    Curse = {
        DeathCause = "呪い殺された。",
        Message = function(entity)
            return ("%sは呪いの力で死んだ。"):format(_.name(entity))
        end,
    },
    Burden = {
        Backpack = "荷物",
        DeathCause = function(itemName)
            return ("%sの重さに耐え切れず潰れた。"):format(itemName)
        end,
        Message = function(entity, item)
            return ("%sは%sの重さに耐え切れず死んだ。"):format(_.name(entity), _.name(item))
        end,
    },
    Stairs = {
        DeathCause = "階段から転げ落ちて亡くなった。",
        Message = function(entity)
            return ("%sは階段から転げ落ちて死んだ。"):format(_.name(entity))
        end,
    },
    Performance = {
        DeathCause = "演奏中に激怒した聴衆に殺された。",
        Message = function(entity)
            return ("%sは聴衆に殺された。"):format(_.name(entity))
        end,
    },
    Burning = {
        DeathCause = "焼けて消滅した。",
        Message = function(entity)
            return ("%sは焼け死んだ。"):format(_.name(entity))
        end,
    },
    UnseedHand = {
        DeathCause = "見えざる手に葬られた。",
        Message = function(entity)
            return ("%sは見えざる手に葬られた。"):format(_.name(entity))
        end,
    },
    FoodPoisoning = {
        DeathCause = "食中毒で倒れた。",
        Message = function(entity)
            return ("%sは食中毒で死んだ。"):format(_.name(entity))
        end,
    },
    Bleeding = {
        DeathCause = "血を流しすぎて死んだ。",
        Message = function(entity)
            return ("%sは出血多量で死んだ。"):format(_.name(entity))
        end,
    },
    Ether = {
        DeathCause = "エーテルの病に倒れた。",
        Message = function(entity)
            return ("%sはエーテルに侵食され死んだ。"):format(_.name(entity))
        end,
    },
    Acid = {
        DeathCause = "溶けて液体になった。",
        Message = function(entity)
            return ("%sは溶けて液体になった。"):format(_.name(entity))
        end,
    },
    Suicide = {
        DeathCause = "自殺した。",
        Message = function(entity)
            return ("%sはバラバラになった。"):format(_.name(entity))
        end,
    },
    Nuke = {
        DeathCause = "核爆発に巻き込まれて死んだ。",
        Message = function(entity)
            return ("%sは核爆発に巻き込まれて塵となった。"):format(_.name(entity))
        end,
    },
    IronMaiden = {
        DeathCause = "アイアンメイデンにはさまれて死んだ。",
        Message = function(entity)
            return ("%sはアイアンメイデンの中で串刺しになって果てた。"):format(_.name(entity))
        end,
    },
    Guillotine = {
        DeathCause = "ギロチンで首を落とされて死んだ。",
        Message = function(entity)
            return ("%sはギロチンで首をちょんぎられて死んだ。"):format(_.name(entity))
        end,
    },
    Hanging = {
        DeathCause = "首を吊った。",
        Message = function(entity)
            return ("%sは首を吊った。"):format(_.name(entity))
        end,
    },
    Mochi = {
        DeathCause = "もちを喉に詰まらせて死んだ。",
        Message = function(entity)
            return ("%sはもちを喉に詰まらせて死んだ。"):format(_.name(entity))
        end,
    },
}
