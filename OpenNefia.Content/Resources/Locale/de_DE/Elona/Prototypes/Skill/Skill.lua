OpenNefia.Prototypes.Elona.Skill.Elona = {
    StatStrength = {
        Name = "Strength",
        ShortName = "STR",

        OnDecrease = function(entity)
            return ("%s muscles soften."):format(_.possessive(entity))
        end,
        InIncrease = function(entity)
            return ("%s muscles feel stronger."):format(_.possessive(entity))
        end,
    },
    StatConstitution = {
        Name = "Constitution",
        ShortName = "CON",

        OnDecrease = function(entity)
            return ("%s lose%s patience."):format(_.name(entity), _.s(entity))
        end,
        OnIncrease = function(entity)
            return ("%s begin%s to feel good when being hit hard."):format(_.name(entity), _.s(entity))
        end,
    },
    StatDexterity = {
        Name = "Dexterity",
        ShortName = "DEX",

        OnDecrease = function(entity)
            return ("%s become%s clumsy."):format(_.name(entity), _.s(entity))
        end,
        OnIncrease = function(entity)
            return ("%s become%s dexterous."):format(_.name(entity), _.s(entity))
        end,
    },
    StatPerception = {
        Name = "Perception",
        ShortName = "PER",

        OnDecrease = function(entity)
            return ("%s %s getting out of touch with the world."):format(_.name(entity), _.is(entity))
        end,
        OnIncrease = function(entity)
            return ("%s feel%s more in touch with the world."):format(_.name(entity), _.s(entity))
        end,
    },
    StatLearning = {
        Name = "Learning",
        ShortName = "LER",

        OnDecrease = function(entity)
            return ("%s lose%s curiosity."):format(_.name(entity), _.s(entity))
        end,
        OnIncrease = function(entity)
            return ("%s feel%s studious."):format(_.name(entity), _.s(entity))
        end,
    },
    StatWill = {
        Name = "Will",
        ShortName = "WIL",

        OnDecrease = function(entity)
            return ("%s will softens."):format(_.possessive(entity))
        end,
        OnIncrease = function(entity)
            return ("%s will hardens."):format(_.possessive(entity))
        end,
    },
    StatMagic = {
        Name = "Magic",
        ShortName = "MAG",

        OnDecrease = function(entity)
            return ("%s magic degrades."):format(_.possessive(entity))
        end,
        OnIncrease = function(entity)
            return ("%s magic improves."):format(_.possessive(entity))
        end,
    },
    StatCharisma = {
        Name = "Charisma",
        ShortName = "CHR",

        OnDecrease = function(entity)
            return ("%s start%s to avoid eyes of people."):format(_.name(entity), _.s(entity))
        end,
        OnIncrease = function(entity)
            return ("%s enjoy%s showing off %s body."):format(_.name(entity), _.s(entity), _.his(entity))
        end,
    },
    StatSpeed = {
        Name = "Speed",

        OnDecrease = function(entity)
            return ("%s speed decreases."):format(_.possessive(entity))
        end,
        OnIncrease = function(entity)
            return ("%s speed increases."):format(_.possessive(entity))
        end,
    },
    StatLuck = {
        Name = "Luck",

        OnDecrease = function(entity)
            return ("%s become%s unlucky."):format(_.name(entity), _.s(entity))
        end,
        OnIncrease = function(entity)
            return ("%s become%s lucky."):format(_.name(entity), _.s(entity))
        end,
    },
    StatLife = {
        Name = "Life",

        OnDecrease = function(entity)
            return ("%s life force decreases."):format(_.possessive(entity))
        end,
        OnIncrease = function(entity)
            return ("%s life force increases."):format(_.possessive(entity))
        end,
    },
    StatMana = {
        Name = "Mana",

        OnDecrease = function(entity)
            return ("%s mana decreases."):format(_.possessive(entity))
        end,
        OnIncrease = function(entity)
            return ("%s mana increases."):format(_.possessive(entity))
        end,
    },

    ActionAbsorbMagic = {
        Description = "Heal MP",
        Name = "Absorb Magic",
    },
    SpellAcidGround = {
        Description = "Create acid grounds",
        Name = "Acid Ground",
    },
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
    },
    Blunt = {
        Description = "Indicates your skill with blunt weapons.",
        Name = "Blunt",
    },
    SpellBuffBoost = {
        Name = "Boost",
    },
    Bow = {
        Description = "Indicates your skill with bows.",
        Name = "Bow",
    },
    Carpentry = {
        Description = "Skill to cut trees and manufcture products.",
        Name = "Carpentry",
    },
    Casting = {
        Description = "Reduces the chance of casting failure.",
        Name = "Casting",
    },
    ActionChange = {
        Description = "Change target",
        Name = "Change",
    },
    SpellChaosBall = {
        Description = "Surround(Chaos)",
        Name = "Chaos Ball",
    },
    ActionChaosBreath = {
        Description = "Breath(Chaos)",
        Name = "Chaos Breath",
    },
    SpellChaosEye = {
        Description = "Target(Chaos)",
        Name = "Chaos eye",
    },
    ActionCheer = {
        Description = "Strengthen allies",
        Name = "Cheer",
    },
    ActionColdBreath = {
        Description = "Breath(Cold)",
        Name = "Cold Breath",
    },
    SpellBuffContingency = {
        Name = "Contingency",
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
    },
    SpellCrystalSpear = {
        Description = "Target(Magic)",
        Name = "Crystal Spear",
    },
    SpellCureOfEris = {
        Description = "Heal self",
        Name = "Cure of Eris",
    },
    SpellCureOfJure = {
        Description = "Heal self",
        Name = "Cure of Jure",
    },
    ActionCurse = {
        Description = "Curse target",
        Name = "Curse",
    },
    SpellDarkEye = {
        Description = "Target(Darkness)",
        Name = "Dark eye",
    },
    SpellDarknessBolt = {
        Description = "Line(Darkness)",
        Name = "Darkness Bolt",
    },
    ActionDarknessBreath = {
        Description = "Breath(Darkness)",
        Name = "Darkness Breath",
    },
    SpellBuffDeathWord = {
        Name = "Death Word",
    },
    ActionDecapitation = {
        Description = "Kill target",
        Name = "Decapitation",
    },
    Detection = {
        Description = "It is used to search hidden locations and traps.",
        Name = "Detection",
    },
    ActionDimensionalMove = {
        Description = "Teleport self",
        Name = "Dimensional Move",
    },
    DisarmTrap = {
        Description = "Allows you to disarm harder traps.",
        Name = "Disarm Trap",
    },
    ActionDistantAttack4 = {
        Name = "Distant Attack",
    },
    ActionDistantAttack7 = {
        Name = "Distant Attack",
    },
    SpellBuffDivineWisdom = {
        Name = "Divine Wisdom",
    },
    SpellDominate = {
        Description = "Dominate target",
        Name = "Dominate",
    },
    SpellDoorCreation = {
        Description = "Create doors",
        Name = "Door Creation",
    },
    ActionDrainBlood = {
        Description = "Drain HP",
        Name = "Drain Blood",
    },
    ActionDrawCharge = {
        Description = "Draw charges",
        Name = "Draw Charge",
    },
    ActionDrawShadow = {
        Description = "Draw target",
        Name = "Draw Shadow",
    },
    ActionDropMine = {
        Description = "Set Mine",
        Name = "Drop Mine",
    },
    DualWield = {
        Description = "Used when wielding two weapoms at the same time.",
        Name = "Dual Wield",
    },
    SpellBuffElementScar = {
        Name = "Element Scar",
    },
    SpellBuffElementalShield = {
        Name = "Elemental Shield",
    },
    ActionEtherGround = {
        Description = "Create ether grounds",
        Name = "Ether Ground",
    },
    Evasion = {
        Description = "Increases your chance of evading enemy attacks.",
        Name = "Evasion",
    },
    ActionEyeOfDimness = {
        Description = "Dim target",
        Name = "Eye of dimness",
    },
    ActionEyeOfEther = {
        Description = "Corrupt target",
        Name = "Eye of Ether",
    },
    ActionEyeOfInsanity = {
        Description = "Craze target",
        Name = "Eye of Insanity",
    },
    ActionEyeOfMana = {
        Description = "Damage MP target",
        Name = "Eye of Mana",
    },
    EyeOfMind = {
        Description = "Increases your chance to deliver critical hits.",
        Name = "Eye of Mind",
    },
    ActionEyeOfMutation = {
        Description = "Mutate target",
        Name = "Eye of Mutation",
    },
    Faith = {
        Description = "Gets you closer to god.",
        EnchantmentDescription = function(item, power)
            return ("%s makes you pious."):format(_.he(item))
        end,
        Name = "Faith",
    },
    ActionFillCharge = {
        Description = "Restore charges",
        Name = "Fill Charge",
    },
    SpellFireBall = {
        Description = "Surround(Fire)",
        Name = "Fire Ball",
    },
    SpellFireBolt = {
        Description = "Line(Fire)",
        Name = "Fire Bolt",
    },
    ActionFireBreath = {
        Description = "Breath(Fire)",
        Name = "Fire Breath",
    },
    SpellFireWall = {
        Description = "Create fire grounds",
        Name = "Fire Wall",
    },
    Firearm = {
        Description = "Indicates your skill with firearms.",
        Name = "Firearm",
    },
    Fishing = {
        Description = "Displays your fishing skill.",
        EnchantmentDescription = function(item, power)
            return ("%s makes you a better fisher."):format(_.he(item))
        end,
        Name = "Fishing",
    },
    SpellFourDimensionalPocket = {
        Description = "Summon 4-Dimensional Pocket",
        Name = "4-Dimensional Pocket",
    },
    Gardening = {
        Description = "Skill to plant seeds and gather harvests.",
        Name = "Gardening",
    },
    GeneEngineer = {
        Description = "Allows you to control genes.",
        Name = "Gene Engineer",
    },
    SpellGravity = {
        Description = "Create gravity",
        Name = "Gravity",
    },
    GreaterEvasion = {
        Description = "Makes you able to evade inaccurate attacks.",
        Name = "Greater Evasion",
    },
    ActionGrenade = {
        Description = "Surround(Sound)",
        Name = "Grenade",
    },
    ActionHarvestMana = {
        Description = "Restore MP",
        Name = "Harvest Mana",
    },
    SpellHealCritical = {
        Description = "Heal self",
        Name = "Heal Critical",
    },
    SpellHealLight = {
        Description = "Heal self",
        Name = "Heal Light",
    },
    Healing = {
        Description = "Gradually heals your wounds.",
        Name = "Healing",
    },
    SpellHealingRain = {
        Description = "Heal area",
        Name = "Healing Rain",
    },
    SpellHealingTouch = {
        Description = "Heal target",
        Name = "Healing Touch",
    },
    HeavyArmor = {
        Description = "Skill to effectively act while wearing heavy armor.",
        Name = "Heavy Armor",
    },
    SpellBuffHero = {
        Name = "Hero",
    },
    SpellHolyLight = {
        Description = "Remove one hex",
        Name = "Holy Light",
    },
    SpellBuffHolyShield = {
        Name = "Holy Shield",
    },
    SpellBuffHolyVeil = {
        Name = "Holy Veil",
    },
    SpellIceBall = {
        Description = "Surround(Cold)",
        Name = "Ice Ball",
    },
    SpellIceBolt = {
        Description = "Line(Cold)",
        Name = "Ice Bolt",
    },
    SpellIdentify = {
        Description = "Identify one item",
        Name = "Identify",
    },
    SpellBuffIncognito = {
        Name = "Incognito",
    },
    ActionInsult = {
        Description = "Insult target",
        Name = "Insult",
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
    SpellLightningBolt = {
        Description = "Line(Lightning)",
        Name = "Lightning Bolt",
    },
    ActionLightningBreath = {
        Description = "Breath(Lightning)",
        Name = "Lightning Breath",
    },
    Literacy = {
        Description = "Allows you to read difficult books.",
        EnchantmentDescription = function(item, power)
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
    },
    SpellBuffLulwysTrick = {
        Name = "Lulwy's Trick",
    },
    MagicCapacity = {
        Description = "Reduces kickback damage from over casting.",
        Name = "Magic Capacity",
    },
    SpellMagicDart = {
        Description = "Target(Magic)",
        Name = "Magic Dart",
    },
    MagicDevice = {
        Description = "Improves effectiveness of magic devices.",
        Name = "Magic Device",
    },
    SpellMagicMap = {
        Description = "Reveal surround map",
        Name = "Magic Map",
    },
    SpellMagicStorm = {
        Description = "Surround(Magic)",
        Name = "Magic Storm",
    },
    ActionManisDisassembly = {
        Description = "Almost kill target",
        Name = "Mani's Disassembly",
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
        EnchantmentDescription = function(item, power)
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
        EnchantmentDescription = function(item, power)
            return ("%s enhances your memory."):format(_.he(item))
        end,
        Name = "Memorization",
    },
    SpellMeteor = {
        Description = "Massive Attack",
        Name = "Meteor",
    },
    ActionMewmewmew = {
        Description = "?",
        Name = "Mewmewmew!",
    },
    SpellMindBolt = {
        Description = "Line(Mind)",
        Name = "Mind Bolt",
    },
    ActionMindBreath = {
        Description = "Breath(Mind)",
        Name = "Mind Breath",
    },
    Mining = {
        Description = "Shows how good of a miner you are.",
        Name = "Mining",
    },
    ActionMirror = {
        Description = "Know self",
        Name = "Mirror",
    },
    SpellMistOfDarkness = {
        Description = "Create mist",
        Name = "Mist of Darkness",
    },
    SpellBuffMistOfFrailness = {
        Name = "Mist of frailness",
    },
    SpellBuffMistOfSilence = {
        Name = "Mist of Silence",
    },
    SpellMutation = {
        Description = "Mutate self",
        Name = "Mutation",
    },
    Negotiation = {
        Description = "Convinces someone to give you better deals.",
        Name = "Negotiation",
    },
    SpellNerveArrow = {
        Description = "Target(Nerve)",
        Name = "Nerve Arrow",
    },
    ActionNerveBreath = {
        Description = "Breath(Nerve)",
        Name = "Nerve Breath",
    },
    SpellNetherArrow = {
        Description = "Target(Nether)",
        Name = "Nether Arrow",
    },
    ActionNetherBreath = {
        Description = "Breath(Nether)",
        Name = "Nether Breath",
    },
    SpellBuffNightmare = {
        Name = "Nightmare",
    },
    SpellOracle = {
        Description = "Reveal artifacts",
        Name = "Oracle",
    },
    Performer = {
        Description = "Shows how good of a performer you are.",
        Name = "Performer",
    },
    Pickpocket = {
        Description = "Shows how good of a thief you are.",
        Name = "Pickpocket",
    },
    ActionPoisonBreath = {
        Description = "Breath(Poison)",
        Name = "Poison Breath",
    },
    Polearm = {
        Description = "Indicates your skill with polearms.",
        Name = "Polearm",
    },
    ActionPowerBreath = {
        Description = "Breath",
        Name = "Power Breath",
    },
    ActionPrayerOfJure = {
        Description = "Heal HP self",
        Name = "Prayer of Jure",
    },
    ActionPregnant = {
        Description = "Pregnant target",
        Name = "Pregnant",
    },
    SpellBuffPunishment = {
        Name = "Punishment",
    },
    SpellRagingRoar = {
        Description = "Surround(Sound)",
        Name = "Raging Roar",
    },
    ActionRainOfSanity = {
        Description = "Cure insane area",
        Name = "Rain of sanity",
    },
    SpellBuffRegeneration = {
        Name = "Regeneration",
    },
    SpellRestoreBody = {
        Description = "Restore physical",
        Name = "Restore Body",
    },
    SpellRestoreSpirit = {
        Description = "Restore mind",
        Name = "Restore Spirit",
    },
    SpellResurrection = {
        Description = "Resurrect dead",
        Name = "Resurrection",
    },
    SpellReturn = {
        Description = "Return",
        Name = "Return",
    },
    Riding = {
        Description = "Allows you to ride creatures.",
        Name = "Riding",
    },
    ActionScavenge = {
        Description = "Steal food",
        Name = "Scavenge",
    },
    Scythe = {
        Description = "Indicates your skill with sycthes.",
        Name = "Scythe",
    },
    SpellSenseObject = {
        Description = "Reveal nearby objects",
        Name = "Sense Object",
    },
    SenseQuality = {
        Description = "Allows you to sense the quality of stuff.",
        Name = "Sense Quality",
    },
    ActionShadowStep = {
        Description = "Teleport to target",
        Name = "Shadow Step",
    },
    Shield = {
        Description = "Increases the effectivness of using shields.",
        Name = "Shield",
    },
    ShortSword = {
        Description = "Indicates your skill with short swords.",
        Name = "Short Sword",
    },
    SpellShortTeleport = {
        Description = "Teleport self",
        Name = "Short Teleport",
    },
    SpellBuffSlow = {
        Name = "Slow",
    },
    ActionSoundBreath = {
        Description = "Breath(Sound)",
        Name = "Sound Breath",
    },
    SpellBuffSpeed = {
        Name = "Speed",
    },
    Stave = {
        Description = "Indicates your skill with staves.",
        Name = "Stave",
    },
    Stealth = {
        Description = "Allows you to move quietly.",
        Name = "Stealth",
    },
    ActionSuicideAttack = {
        Description = "Suicide",
        Name = "Suicide Attack",
    },
    ActionSummonCats = {
        Description = "Summon cats",
        Name = "Summon Cats",
    },
    ActionSummonFire = {
        Description = "Summon fire creatures",
        Name = "Summon Fire",
    },
    SpellSummonMonsters = {
        Description = "Summon hostile creatures",
        Name = "Summon Monsters",
    },
    ActionSummonPawn = {
        Description = "Summon pieces",
        Name = "Summon Pawn",
    },
    ActionSummonSister = {
        Description = "Summon sisters",
        Name = "Summon sister",
    },
    SpellSummonWild = {
        Description = "Summon wild creatures",
        Name = "Summon Wild",
    },
    ActionSummonYeek = {
        Description = "Summon Yeeks",
        Name = "Summon Yeek",
    },
    ActionSuspiciousHand = {
        Description = "Steal from target",
        Name = "Suspicious Hand",
    },
    ActionSwarm = {
        Description = "Attack circle",
        Name = "Swarm",
    },
    Tactics = {
        Description = "Increases melee damage.",
        Name = "Tactics",
    },
    Tailoring = {
        Description = "Skill to sew materials and manufucture products.",
        EnchantmentDescription = function(item, power)
            return ("%s makes you a better tailor."):format(_.he(item))
        end,
        Name = "Tailoring",
    },
    SpellTeleport = {
        Description = "Teleport self",
        Name = "Teleport",
    },
    SpellTeleportOther = {
        Description = "Teleport target",
        Name = "Teleport Other",
    },
    Throwing = {
        Description = "Indicates your skill with throwing objects.",
        Name = "Throwing",
    },
    ActionTouchOfFear = {
        Description = "Fear target",
        Name = "Touch of Fear",
    },
    ActionTouchOfHunger = {
        Description = "Starve target",
        Name = "Touch of Hunger",
    },
    ActionTouchOfNerve = {
        Description = "Paralyze target",
        Name = "Touch of Nerve",
    },
    ActionTouchOfPoison = {
        Description = "Poison target",
        Name = "Touch of Poison",
    },
    ActionTouchOfSleep = {
        Description = "Sleep target",
        Name = "Touch of Sleep",
    },
    ActionTouchOfWeakness = {
        Description = "Weaken target",
        Name = "Touch of Weakness",
    },
    Traveling = {
        Description = "Allows you to receive more EXP from traveling.",
        Name = "Traveling",
    },
    TwoHand = {
        Description = "Used when wielding a weapon with both hands.",
        Name = "Two Hand",
    },
    SpellUncurse = {
        Description = "Uncurse items",
        Name = "Uncurse",
    },
    ActionVanish = {
        Description = "Escape self.",
        Name = "Vanish",
    },
    SpellVanquishHex = {
        Description = "Remove all hexes",
        Name = "Vanquish Hex",
    },
    SpellWallCreation = {
        Description = "Create walls",
        Name = "Wall Creation",
    },
    SpellWeb = {
        Description = "Create webs",
        Name = "Web",
    },
    WeightLifting = {
        Description = "Allows you to carry more stuff.",
        Name = "Weight Lifting",
    },
    SpellWish = {
        Description = "Wish",
        Name = "Wish",
    },
    SpellWizardsHarvest = {
        Description = "Random harvest",
        Name = "Wizard's Harvest",
    },
}
