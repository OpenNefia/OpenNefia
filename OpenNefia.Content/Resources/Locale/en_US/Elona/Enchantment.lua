Elona.Enchantment = {
    PowerUnit = "#",

    Item = {
        ModifyAttribute = {
            Equipment = {
                Increases = function(item, wielder, skillName, power)
                    return ("%s increases %s %s by %s."):format(_.he(item), _.possessive(wielder), skillName, power)
                end,
                Decreases = function(item, wielder, skillName, power)
                    return ("%s decreases %s %s by %s."):format(_.he(item), _.possessive(wielder), skillName, power)
                end,
            },
            Food = {
                Increases = function(item, wielder, skillName, power)
                    return ("%s has essential nutrients to enhance %s %s."):format(
                        _.he(item),
                        _.possessive(wielder),
                        skillName
                    )
                end,
                Decreases = function(item, wielder, skillName, power)
                    return ("%s has which deteriorates %s %s."):format(_.he(item), _.possessive(wielder), skillName)
                end,
            },
            Eaten = {
                Increases = function(chara, skillName)
                    return ("%s %s develops."):format(_.possessive(chara), skillName)
                end,
                Decreases = function(chara, skillName)
                    return ("%s %s deteriorates."):format(_.possessive(chara), skillName)
                end,
            },
        },

        ModifyResistance = {
            Increases = function(item, wielder, elementName)
                return ("%s grants %s resistance to %s."):format(_.he(item), _.posessive(wielder), elementName)
            end,
            Decreases = function(item, wielder, elementName)
                return ("%s weakens %s resistance to %s."):format(_.he(item), _.possessive(wielder), elementName)
            end,
        },

        ModifySkill = {
            Increases = function(item, wielder, skillName, power)
                return ("%s improves %s %s skill."):format(_.he(item), _.possessive(wielder), skillName)
            end,
            Decreases = function(item, wielder, skillName, power)
                return ("%s decreases %s %s skill."):format(_.he(item), _.possessive(wielder), skillName)
            end,
        },

        SustainAttribute = {
            Equipment = function(item, wielder, skillName, power)
                return ("%s maintains %s."):format(_.he(item), skillName)
            end,
            Food = function(item, wielder, skillName, power)
                return ("%s can help %s exercise %s %s faster."):format(
                    _.he(item),
                    _.name(wielder, true),
                    _.possessive(wielder),
                    skillName
                )
            end,
            Eaten = function(chara, skillName)
                return ("%s %s enters a period of rapid growth."):format(_.possessive(chara), skillName)
            end,
        },

        ElementalDamage = {
            Description = function(item, wielder, elementName, power)
                return ("%s deals %s damage."):format(_.he(item), elementName)
            end,
        },

        InvokeSpell = {
            Description = function(item, wielder, spellName, power)
                return ("%s invokes %s."):format(_.he(item), spellName)
            end,
        },

        SuckBlood = {
            BloodSucked = function(entity)
                return ("Something sucks %s blood."):format(_.possessive(entity))
            end,
        },

        SuckExperience = {
            ExperienceReduced = function(entity)
                return ("%s become%s inexperienced."):format(_.name(entity), _.s(entity))
            end,
        },

        SummonCreature = {
            CreatureSummoned = "Several creatures are summoned from a vortex of magic.",
        },
    },

    Ego = {
        Major = {
            Elona = {
                Silence = function(name)
                    return ("%s of silence"):format(name)
                end,
                ResBlind = function(name)
                    return ("%s of resist blind"):format(name)
                end,
                ResConfuse = function(name)
                    return ("%s of resist confusion"):format(name)
                end,
                Fire = function(name)
                    return ("%s of fire"):format(name)
                end,
                Cold = function(name)
                    return ("%s of cold"):format(name)
                end,
                Lightning = function(name)
                    return ("%s of lightning"):format(name)
                end,
                Healer = function(name)
                    return ("%s of healing"):format(name)
                end,
                ResParalyze = function(name)
                    return ("%s of resist paralysis"):format(name)
                end,
                ResFear = function(name)
                    return ("%s of resist fear"):format(name)
                end,
                ResSleep = function(name)
                    return ("%s of resist sleep"):format(name)
                end,
                Defender = function(name)
                    return ("%s of defender"):format(name)
                end,
            },
        },
        Minor = {
            Elona = {
                Singing = function(name)
                    return ("singing %s"):format(name)
                end,
                Servants = function(name)
                    return ("servant's %s"):format(name)
                end,
                Followers = function(name)
                    return ("follower's %s"):format(name)
                end,
                Howling = function(name)
                    return ("howling %s"):format(name)
                end,
                Glowing = function(name)
                    return ("glowing %s"):format(name)
                end,
                Conspicuous = function(name)
                    return ("conspicuous %s"):format(name)
                end,
                Magical = function(name)
                    return ("magical %s"):format(name)
                end,
                Enchanted = function(name)
                    return ("enchanted %s"):format(name)
                end,
                Mighty = function(name)
                    return ("mighty %s"):format(name)
                end,
                Trustworthy = function(name)
                    return ("trustworthy %s"):format(name)
                end,
            },
        },
    },
}
