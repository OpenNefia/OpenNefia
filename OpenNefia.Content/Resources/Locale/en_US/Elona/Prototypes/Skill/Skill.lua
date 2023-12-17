OpenNefia.Prototypes.Elona.Skill.Elona = {
    Alchemy = {
        Description = "Enables you to perform alchemy.",
        Name = "Alchemy",
    },
    Anatomy = {
        Description = "Gives you a better chance of finding dead bodies.",
        Name = "Anatomy",
    },
    Axe = {
        Description = "Indicates your skill with axes.",
        Name = "Axe",
        Damage = {
            WeaponName = "axe",
            VerbPassive = "slash",
            VerbActive = "slash",
        },
    },
    Blunt = {
        Description = "Indicates your skill with blunt weapons.",
        Name = "Blunt",
        Damage = {
            WeaponName = "mace",
            VerbPassive = "smash",
            VerbActive = "smash",
        },
    },
    Bow = {
        Description = "Indicates your skill with bows.",
        Name = "Bow",
        Damage = {
            WeaponName = "bow",
            VerbPassive = "shoot",
            VerbActive = "shoot",
        },
    },
    Carpentry = {
        Description = "Skill to cut trees and manufcture products.",
        Name = "Carpentry",
    },
    Casting = {
        Description = "Reduces the chance of casting failure.",
        Name = "Casting",
    },
    ControlMagic = {
        Description = "Prevents your allies to get hit by your spells.",
        Name = "Control Magic",
    },
    Cooking = {
        Description = "Improves your cooking skill.",
        Name = "Cooking",
    },
    Crossbow = {
        Description = "Indicates your skill with cross bows",
        Name = "Crossbow",
        Damage = {
            WeaponName = "crossbow",
            VerbPassive = "shoot",
            VerbActive = "shoot",
        },
    },
    Detection = {
        Description = "It is used to search hidden locations and traps.",
        Name = "Detection",
    },
    DisarmTrap = {
        Description = "Allows you to disarm harder traps.",
        Name = "Disarm Trap",
    },
    DualWield = {
        Description = "Used when wielding two weapoms at the same time.",
        Name = "Dual Wield",
    },
    Evasion = {
        Description = "Increases your chance of evading enemy attacks.",
        Name = "Evasion",
    },
    EyeOfMind = {
        Description = "Increases your chance to deliver critical hits.",
        Name = "Eye of Mind",
    },
    Faith = {
        Description = "Gets you closer to god.",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s makes you pious."):format(_.he(item))
        end,
        Name = "Faith",
    },
    Firearm = {
        Description = "Indicates your skill with firearms.",
        Name = "Firearm",
        Damage = {
            WeaponName = "gun",
            VerbPassive = "shoot",
            VerbActive = "shoot",
        },
    },
    Fishing = {
        Description = "Displays your fishing skill.",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s makes you a better fisher."):format(_.he(item))
        end,
        Name = "Fishing",
    },
    Gardening = {
        Description = "Skill to plant seeds and gather harvests.",
        Name = "Gardening",
    },
    GeneEngineer = {
        Description = "Allows you to control genes.",
        Name = "Gene Engineer",
    },
    GreaterEvasion = {
        Description = "Makes you able to evade inaccurate attacks.",
        Name = "Greater Evasion",
    },
    Healing = {
        Description = "Gradually heals your wounds.",
        Name = "Healing",
    },
    HeavyArmor = {
        Description = "Skill to effectively act while wearing heavy armor.",
        Name = "Heavy Armor",
    },
    Investing = {
        Description = "Lowers the money needed to invest in shops.",
        Name = "Investing",
    },
    Jeweler = {
        Description = "Skill to process jewels and manufucture products.",
        Name = "Jeweler",
    },
    LightArmor = {
        Description = "Skill to effectively act while wearing light armor.",
        Name = "Light Armor",
    },
    Literacy = {
        Description = "Allows you to read difficult books.",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s makes you literate."):format(_.he(item))
        end,
        Name = "Literacy",
    },
    LockPicking = {
        Description = "Allows you to pick more difficult locks.",
        Name = "Lock Picking",
    },
    LongSword = {
        Description = "Indicates your skill with long swords.",
        Name = "Long Sword",
        Damage = {
            WeaponName = "sword",
            VerbPassive = "slash",
            VerbActive = "slash",
        },
    },
    MagicCapacity = {
        Description = "Reduces kickback damage from over casting.",
        Name = "Magic Capacity",
    },
    MagicDevice = {
        Description = "Improves effectiveness of magic devices.",
        Name = "Magic Device",
    },
    Marksman = {
        Description = "Increases shooting damage.",
        Name = "Marksman",
    },
    MartialArts = {
        Description = "Indicates your skill fighting unarmed.",
        Name = "Martial Arts",
    },
    Meditation = {
        Description = "Gradually restores your magic points.",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s enhances your meditation."):format(_.he(item))
        end,
        Name = "Meditation",
    },
    MediumArmor = {
        Description = "Skill to effectively act while wearing medium armor.",
        Name = "Medium Armor",
    },
    Memorization = {
        Description = "Helps you acquire additional spell stocks.",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s enhances your memory."):format(_.he(item))
        end,
        Name = "Memorization",
    },
    Mining = {
        Description = "Shows how good of a miner you are.",
        Name = "Mining",
    },
    Negotiation = {
        Description = "Convinces someone to give you better deals.",
        Name = "Negotiation",
    },
    Performer = {
        Description = "Shows how good of a performer you are.",
        Name = "Performer",
    },
    Pickpocket = {
        Description = "Shows how good of a thief you are.",
        Name = "Pickpocket",
    },
    Polearm = {
        Description = "Indicates your skill with polearms.",
        Name = "Polearm",
        Damage = {
            WeaponName = "spear",
            VerbPassive = "stab",
            VerbActive = "stab",
        },
    },
    Riding = {
        Description = "Allows you to ride creatures.",
        Name = "Riding",
    },
    Scythe = {
        Description = "Indicates your skill with sycthes.",
        Name = "Scythe",
        Damage = {
            WeaponName = "scythe",
            VerbPassive = "slash",
            VerbActive = "slash",
        },
    },
    SenseQuality = {
        Description = "Allows you to sense the quality of stuff.",
        Name = "Sense Quality",
    },
    Shield = {
        Description = "Increases the effectivness of using shields.",
        Name = "Shield",
    },
    ShortSword = {
        Description = "Indicates your skill with short swords.",
        Name = "Short Sword",
        Damage = {
            WeaponName = "dagger",
            VerbPassive = "stab",
            VerbActive = "stab",
        },
    },
    Stave = {
        Description = "Indicates your skill with staves.",
        Name = "Stave",
        Damage = {
            WeaponName = "staff",
            VerbPassive = "smash",
            VerbActive = "smash",
        },
    },
    Stealth = {
        Description = "Allows you to move quietly.",
        Name = "Stealth",
    },
    Tactics = {
        Description = "Increases melee damage.",
        Name = "Tactics",
    },
    Tailoring = {
        Description = "Skill to sew materials and manufucture products.",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s makes you a better tailor."):format(_.he(item))
        end,
        Name = "Tailoring",
    },
    Throwing = {
        Description = "Indicates your skill with throwing objects.",
        Name = "Throwing",
        Damage = {
            WeaponName = "projectile",
            VerbPassive = "shoot",
            VerbActive = "shoot",
            AttacksActive = function(attacker, verb, target, weapon)
                return ("%s %s%s %s and"):format(_.name(attacker), verb, _.s(attacker), _.name(target))
            end,
        },
    },
    Traveling = {
        Description = "Allows you to receive more EXP from traveling.",
        Name = "Traveling",
    },
    TwoHand = {
        Description = "Used when wielding a weapon with both hands.",
        Name = "Two Hand",
    },
    WeightLifting = {
        Description = "Allows you to carry more stuff.",
        Name = "Weight Lifting",
    },
}
