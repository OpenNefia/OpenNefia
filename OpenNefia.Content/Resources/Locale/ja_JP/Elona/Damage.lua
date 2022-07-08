Elona.Damage = {
    Furthermore = "さらに",
    Attacks = {
        Active = {
            Armed = function(attacker, verb, target, weapon)
                return ("%s%sを%s"):format(_.sore_wa(attacker), _.name(target), verb)
            end,
            Unarmed = function(attacker, verb, target)
                return ("%s%sを%s"):format(_.sore_wa(attacker), _.name(target), verb)
            end,
        },
        Passive = {
            Armed = function(attacker, verb, target, weapon)
                return ("%s%sに%sで%s。"):format(_.sore_wa(target), _.name(attacker), _.name(weapon), verb)
            end,
            Unarmed = function(attacker, verb, target)
                return ("%s%sに%s"):format(_.sore_wa(target), _.name(attacker), verb)
            end,
        },
    },
    Levels = {
        Critically = "致命傷を与えた。",
        Moderately = "傷つけた。",
        Scratch = "かすり傷をつけた。",
        Severely = "深い傷を負わせた。",
        Slightly = "軽い傷を負わせた。",
    },
    Reactions = {
        Screams = function(entity)
            return ("%sは痛手を負った。"):format(_.name(entity))
        end,
        WrithesInPain = function(entity)
            return ("%sは苦痛にもだえた。"):format(_.name(entity))
        end,
        IsSeverelyHurt = function(entity)
            return ("%sは悲痛な叫び声をあげた。"):format(_.name(entity))
        end,
        IsHealed = function(entity)
            return ("%sは回復した。"):format(_.name(entity))
        end,
    },
    Wounded = "は傷ついた。",
    Killed = {
        Active = "殺した。",
        Passive = function(target, attacker)
            return ("%sは死んだ。"):format(_.name(target, attacker))
        end,
    },
    YouFeelSad = "あなたは悲しくなった。",
    MagicReaction = {
        Hurts = function(entity)
            return ("マナの反動が%sの精神を蝕んだ！"):format(_.name(entity))
        end,
    },
    RunsAway = function(entity)
        return ("%sは恐怖して逃げ出した。"):format(_.name(entity))
    end,
    SleepIsDisturbed = function(entity)
        return ("%sは眠りを妨げられた。"):format(_.name(entity))
    end,
}
