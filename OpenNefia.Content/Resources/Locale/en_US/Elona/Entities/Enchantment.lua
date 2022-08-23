OpenNefia.Prototypes.Entity.Elona = {
    EncRandomTeleport = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s causes random teleport."):format(_.he(item))
            end,
        },
    },

    EncSuckBlood = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s sucks blood of the wielder."):format(_.he(item))
            end,
        },
    },

    EncSuckExperience = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s disturbs %s growth."):format(_.he(item), _.possessive(wielder))
            end,
        },
    },

    EncSummonCreature = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s attracts monsters."):format(_.he(item))
            end,
        },
    },

    EncPreventTeleport = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s prevents %s from teleporting."):format(_.he(item), _.name(wielder, true))
            end,
        },
    },

    EncResistBlindness = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s negates the effect of blindness."):format(_.he(item))
            end,
        },
    },

    EncResistParalysis = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s negates the effect of paralysis."):format(_.he(item))
            end,
        },
    },

    EncResistConfusion = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s negates the effect of confusion."):format(_.he(item))
            end,
        },
    },

    EncResistFear = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s negates the effect of fear."):format(_.he(item))
            end,
        },
    },

    EncResistSleep = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s negates the effect of sleep."):format(_.he(item))
            end,
        },
    },

    EncResistPoison = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s negates the effect of poison."):format(_.he(item))
            end,
        },
    },

    EncResistTheft = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s protects %s from thieves."):format(_.he(item), _.name(wielder, true))
            end,
        },
    },

    EncResistRottenFood = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s allows %s to digest rotten food."):format(_.he(item), _.name(wielder, true))
            end,
        },
    },

    EncFastTravel = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s speeds up %s travel progress."):format(_.he(item), _.possessive(wielder))
            end,
        },
    },

    EncResistEtherwind = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s protects %s from Etherwind."):format(_.he(item), _.name(wielder, true))
            end,
        },
    },

    EncResistBadWeather = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s negates the effect of being stranded by bad weather."):format(_.he(item))
            end,
        },
    },

    EncResistPregnancy = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s prevents aliens from entering %s body."):format(_.he(item), _.possessive(wielder))
            end,
        },
    },

    EncFloat = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s floats %s."):format(_.he(item), _.name(wielder, true))
            end,
        },
    },

    EncResistMutation = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s protects %s from mutation."):format(_.he(item), _.name(wielder, true))
            end,
        },
    },

    EncEnhanceSpells = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s enhances %s spells."):format(_.he(item), _.possessive(wielder))
            end,
        },
    },

    EncSeeInvisible = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s allows %s to see invisible creatures."):format(_.he(item), _.name(wielder, true))
            end,
        },
    },

    EncAbsorbStamina = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s absorbs stamina from an enemy."):format(_.he(item))
            end,
        },
    },

    EncRagnarok = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s brings an end."):format(_.he(item))
            end,
        },
    },

    EncAbsorbMana = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s absorbs MP from an enemy."):format(_.he(item))
            end,
        },
    },

    EncPierceChance = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s gives %s a chance to throw an absolute piercing attak."):format(
                    _.he(item),
                    _.name(wielder, true)
                )
            end,
        },
    },

    EncCriticalChance = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s increases %s chance to deliver critical hits."):format(_.he(item), _.possessive(wielder))
            end,
        },
    },

    EncExtraMeleeAttackChance = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s increases the chance of extra melee attack."):format(_.he(item))
            end,
        },
    },

    EncExtraRangedAttackChance = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s increases the chance of extra ranged attack."):format(_.he(item))
            end,
        },
    },

    EncTimeStop = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s occasionally stops time."):format(_.he(item))
            end,
        },
    },

    EncResistCurse = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s protects %s from cursing words."):format(_.he(item), _.name(wielder, true))
            end,
        },
    },

    EncStradivarius = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s increases the quality of reward."):format(_.he(item))
            end,
        },
    },

    EncDamageResistance = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s decreases physical damage %s take%s."):format(
                    _.he(item),
                    _.name(wielder, true),
                    _.s(wielder)
                )
            end,
        },
    },

    EncDamageImmunity = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s sometimes nullifies damage %s take%s."):format(
                    _.he(item),
                    _.name(wielder, true),
                    _.s(wielder)
                )
            end,
        },
    },

    EncDamageReflection = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s deals cut damage to the attacker."):format(_.he(item))
            end,
        },
    },

    EncCuresBleedingQuickly = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s diminishes bleeding."):format(_.he(item))
            end,
        },
    },

    EncCatchesGodSignals = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s catches signals from God."):format(_.he(item))
            end,
        },
    },

    EncDragonBane = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s inflicts massive damage to dragons."):format(_.he(item))
            end,
        },
    },

    EncUndeadBane = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s inflicts massive damage to undeads."):format(_.he(item))
            end,
        },
    },

    EncDetectReligion = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s reveals religion."):format(_.he(item))
            end,
        },
    },

    EncGould = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s makes the audience drunk with haunting tones."):format(_.he(item))
            end,
        },
    },

    EncGodBane = {
        Enchantment = {
            Description = function(item, power, wielder)
                return ("%s inflicts massive damage to Gods."):format(_.he(item))
            end,
        },
    },
}
