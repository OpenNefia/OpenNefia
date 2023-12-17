OpenNefia.Prototypes.Elona.Skill.Elona = {
    AttrStrength = {
        Name = "Strength",
        ShortName = "STR",
        OnDecrease = function(entity)
            return ("%s muscles soften."):format(_.possessive(entity))
        end,
        InIncrease = function(entity)
            return ("%s muscles feel stronger."):format(_.possessive(entity))
        end,
    },
    AttrConstitution = {
        Name = "Constitution",
        ShortName = "CON",
        OnDecrease = function(entity)
            return ("%s lose%s patience."):format(_.name(entity), _.s(entity))
        end,
        OnIncrease = function(entity)
            return ("%s begin%s to feel good when being hit hard."):format(_.name(entity), _.s(entity))
        end,
    },
    AttrDexterity = {
        Name = "Dexterity",
        ShortName = "DEX",
        OnDecrease = function(entity)
            return ("%s become%s clumsy."):format(_.name(entity), _.s(entity))
        end,
        OnIncrease = function(entity)
            return ("%s become%s dexterous."):format(_.name(entity), _.s(entity))
        end,
    },
    AttrPerception = {
        Name = "Perception",
        ShortName = "PER",
        OnDecrease = function(entity)
            return ("%s %s getting out of touch with the world."):format(_.name(entity), _.is(entity))
        end,
        OnIncrease = function(entity)
            return ("%s feel%s more in touch with the world."):format(_.name(entity), _.s(entity))
        end,
    },
    AttrLearning = {
        Name = "Learning",
        ShortName = "LER",
        OnDecrease = function(entity)
            return ("%s lose%s curiosity."):format(_.name(entity), _.s(entity))
        end,
        OnIncrease = function(entity)
            return ("%s feel%s studious."):format(_.name(entity), _.s(entity))
        end,
    },
    AttrWill = {
        Name = "Will",
        ShortName = "WIL",
        OnDecrease = function(entity)
            return ("%s will softens."):format(_.possessive(entity))
        end,
        OnIncrease = function(entity)
            return ("%s will hardens."):format(_.possessive(entity))
        end,
    },
    AttrMagic = {
        Name = "Magic",
        ShortName = "MAG",
        OnDecrease = function(entity)
            return ("%s magic degrades."):format(_.possessive(entity))
        end,
        OnIncrease = function(entity)
            return ("%s magic improves."):format(_.possessive(entity))
        end,
    },
    AttrCharisma = {
        Name = "Charisma",
        ShortName = "CHR",
        OnDecrease = function(entity)
            return ("%s start%s to avoid eyes of people."):format(_.name(entity), _.s(entity))
        end,
        OnIncrease = function(entity)
            return ("%s enjoy%s showing off %s body."):format(_.name(entity), _.s(entity), _.his(entity))
        end,
    },
    AttrSpeed = {
        Name = "Speed",
        OnDecrease = function(entity)
            return ("%s speed decreases."):format(_.possessive(entity))
        end,
        OnIncrease = function(entity)
            return ("%s speed increases."):format(_.possessive(entity))
        end,
    },
    AttrLuck = {
        Name = "Luck",
        OnDecrease = function(entity)
            return ("%s become%s unlucky."):format(_.name(entity), _.s(entity))
        end,
        OnIncrease = function(entity)
            return ("%s become%s lucky."):format(_.name(entity), _.s(entity))
        end,
    },
    AttrLife = {
        Name = "Life",
        OnDecrease = function(entity)
            return ("%s life force decreases."):format(_.possessive(entity))
        end,
        OnIncrease = function(entity)
            return ("%s life force increases."):format(_.possessive(entity))
        end,
    },
    AttrMana = {
        Name = "Mana",
        OnDecrease = function(entity)
            return ("%s mana decreases."):format(_.possessive(entity))
        end,
        OnIncrease = function(entity)
            return ("%s mana increases."):format(_.possessive(entity))
        end,
    },
}
