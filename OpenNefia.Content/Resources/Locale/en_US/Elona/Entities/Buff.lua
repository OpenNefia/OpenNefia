OpenNefia.Prototypes.Entity.Elona = {
    BuffHolyShield = {
        Buff = {
            Apply = function(_1)
                return ("%s begin%s to shine."):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1)
                return ("Increases PV by %s/RES+ fear"):format(_1)
            end,
        },
        MetaData = {
            Name = "Holy Shield",
        },
    },
    BuffMistOfSilence = {
        Buff = {
            Apply = function(_1)
                return ("%s get%s surrounded by hazy mist."):format(_.name(_1), _.s(_1))
            end,
            Description = "Inhibits casting",
        },
        MetaData = {
            Name = "Mist of Silence",
        },
    },
    BuffRegeneration = {
        Buff = {
            Apply = function(_1)
                return ("%s start%s to regenerate."):format(_.name(_1), _.s(_1))
            end,
            Description = "Enhances regeneration",
        },
        MetaData = {
            Name = "Regeneration",
        },
    },
    BuffElementalShield = {
        Buff = {
            Apply = function(_1)
                return ("%s obtain%s resistance to element."):format(_.name(_1), _.s(_1))
            end,
            Description = "RES+ fire,cold,lightning",
        },
        MetaData = {
            Name = "Elemental Shield",
        },
    },
    BuffSpeed = {
        Buff = {
            Apply = function(_1)
                return ("%s speed%s up."):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1)
                return ("Increases speed by %s"):format(_1)
            end,
        },
        MetaData = {
            Name = "Speed",
        },
    },
    BuffSlow = {
        Buff = {
            Apply = function(_1)
                return ("%s slow%s down."):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1)
                return ("Decreases speed by %s"):format(_1)
            end,
        },
        MetaData = {
            Name = "Slow",
        },
    },
    BuffHero = {
        Buff = {
            Apply = function(_1)
                return ("%s feel%s heroic."):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1)
                return ("Increases STR,DEX by %s/RES+ fear,confusion"):format(_1)
            end,
        },
        MetaData = {
            Name = "Hero",
        },
    },
    BuffMistOfFrailness = {
        Buff = {
            Apply = function(_1)
                return ("%s feel%s weak."):format(_.name(_1), _.s(_1))
            end,
            Description = "Halves DV and PV",
        },
        MetaData = {
            Name = "Mist of Frailness",
        },
    },
    BuffElementScar = {
        Buff = {
            Apply = function(_1)
                return ("%s lose%s resistance to element."):format(_.name(_1), _.s(_1))
            end,
            Description = "RES- fire,cold,lightning",
        },
        MetaData = {
            Name = "Element Scar",
        },
    },
    BuffHolyVeil = {
        Buff = {
            Apply = function(_1)
                return ("%s receive%s holy protection."):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1)
                return ("grants hex protection(power:%s)"):format(_1)
            end,
        },
        MetaData = {
            Name = "Holy Veil",
        },
    },
    BuffNightmare = {
        Buff = {
            Apply = function(_1)
                return ("%s start%s to suffer."):format(_.name(_1), _.s(_1))
            end,
            Description = "RES- mind,nerve",
        },
        MetaData = {
            Name = "Nightmare",
        },
    },
    BuffDivineWisdom = {
        Buff = {
            Apply = function(_1)
                return ("%s start%s to think clearly."):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1, _2)
                return ("Increases LER,MAG by %s, literacy skill by %s"):format(_1, _2)
            end,
        },
        MetaData = {
            Name = "Divine Wisdom",
        },
    },
    BuffPunishment = {
        Buff = {
            Apply = function(_1)
                return ("%s incur%s the wrath of God."):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1, _2)
                return ("Decreases speed by %s, PV by %s%%"):format(_1, _2)
            end,
        },
        MetaData = {
            Name = "Punishment",
        },
    },
    BuffLulwysTrick = {
        Buff = {
            Apply = function(_1)
                return ("%s repeat%s the name of Lulwy."):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1)
                return ("Increases speed by %s"):format(_1)
            end,
        },
        MetaData = {
            Name = "Lulwy's Trick",
        },
    },
    BuffIncognito = {
        Buff = {
            Apply = function(_1)
                return ("%s start%s to disguise."):format(_.name(_1), _.s(_1))
            end,
            Description = "Grants new identity",
        },
        MetaData = {
            Name = "Incognito",
        },
    },
    BuffDeathWord = {
        Buff = {
            Apply = function(_1)
                return ("%s receive%s death verdict."):format(_.name(_1), _.s(_1))
            end,
            Description = "Guaranteed death when the hex ends",
        },
        MetaData = {
            Name = "Death Word",
        },
    },
    BuffBoost = {
        Buff = {
            Apply = function(_1)
                return ("%s gain%s massive power."):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1)
                return ("Increases speed by %s/Boosts physical attributes"):format(_1)
            end,
        },
        MetaData = {
            Name = "Boost",
        },
    },
    BuffContingency = {
        Buff = {
            Apply = function(_1)
                return ("%s set%s up contracts with the Reaper."):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1)
                return ("%s%% chances taking a lethal damage heals you instead"):format(_1)
            end,
        },
        MetaData = {
            Name = "Contingency",
        },
    },
    BuffLucky = {
        Buff = {
            Apply = function(_1)
                return ("%s feel%s very lucky today!"):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1)
                return ("Increase luck by %s."):format(_1)
            end,
        },
        MetaData = {
            Name = "Luck",
        },
    },
    BuffFoodStrength = {
        Buff = {
            Apply = function(_1)
                return ("%s feel%s rapid STR growth."):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1)
                return ("Increases the growth rate Strength by %s"):format(_1)
            end,
        },
        MetaData = {
            Name = "Grow Strength",
        },
    },
    BuffFoodConstitution = {
        Buff = {
            Apply = function(_1)
                return ("%s feel%s rapid CON growth."):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1)
                return ("Increases the growth rate Constitution by %s"):format(_1)
            end,
        },
        MetaData = {
            Name = "Grow Constitution",
        },
    },
    BuffFoodDexterity = {
        Buff = {
            Apply = function(_1)
                return ("%s feel%s rapid DEX growth."):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1)
                return ("Increases the growth rate Dexterity by %s"):format(_1)
            end,
        },
        MetaData = {
            Name = "Grow Dexterity",
        },
    },
    BuffFoodPerception = {
        Buff = {
            Apply = function(_1)
                return ("%s feel%s rapid PER growth."):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1)
                return ("Increases the growth rate Perception by %s"):format(_1)
            end,
        },
        MetaData = {
            Name = "Grow Perception",
        },
    },
    BuffFoodLearning = {
        Buff = {
            Apply = function(_1)
                return ("%s feel%s rapid LER growth."):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1)
                return ("Increases the growth rate Learning by %s"):format(_1)
            end,
        },
        MetaData = {
            Name = "Grow Learning",
        },
    },
    BuffFoodWill = {
        Buff = {
            Apply = function(_1)
                return ("%s feel%s rapid WIL growth."):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1)
                return ("Increases the growth rate Will by %s"):format(_1)
            end,
        },
        MetaData = {
            Name = "Grow Will",
        },
    },
    BuffFoodMagic = {
        Buff = {
            Apply = function(_1)
                return ("%s feel%s rapid MAG growth."):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1)
                return ("Increases the growth rate Magic by %s"):format(_1)
            end,
        },
        MetaData = {
            Name = "Grow Magic",
        },
    },
    BuffFoodCharisma = {
        Buff = {
            Apply = function(_1)
                return ("%s feel%s rapid CHR growth."):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1)
                return ("Increases the growth rate Charisma by %s"):format(_1)
            end,
        },
        MetaData = {
            Name = "Grow Charisma",
        },
    },
    BuffFoodSpeed = {
        Buff = {
            Apply = function(_1)
                return ("%s feel%s rapid SPD growth."):format(_.name(_1), _.s(_1))
            end,
            Description = function(_1)
                return ("Increases the growth rate Speed by %s"):format(_1)
            end,
        },
        MetaData = {
            Name = "Grow Speed",
        },
    },
}
