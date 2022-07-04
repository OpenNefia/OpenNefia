Elona.Combat = {
    PhysicalAttack = {
        CriticalHit = "会心の一撃！ ",
        Vorpal = {
            Melee = " *シャキーン* ",
            Ranged = " *ズバシュッ* ",
        },
        Miss = {
            Ally = function(attacker, target)
                return ("%s%sの攻撃を華麗に避けた。"):format(_.kare_wa(target), _.name(attacker))
            end,
            Other = function(attacker, target)
                return ("%s攻撃を華麗にかわされた。"):format(_.kare_wa(attacker))
            end,
        },
        Evade = {
            Ally = function(attacker, target)
                return ("%s%sの攻撃を避けた。"):format(_.kare_wa(target), _.name(attacker))
            end,
            Other = function(attacker, target)
                return ("%s攻撃をかわされた。"):format(_.kare_wa(attacker))
            end,
        },
        Furthermore = "さらに",
        WieldsProudly = function(wielder, itemName)
            return ("%sは%sを誇らしげに構えた。"):format(_.name(wielder), itemName)
        end,
    },
    Damage = {
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
        RunsAway = function(entity)
            return ("%sは恐怖して逃げ出した。"):format(_.name(entity))
        end,
        SleepIsDisturbed = function(entity)
            return ("%sは眠りを妨げられた。"):format(_.name(entity))
        end,
    },
}
