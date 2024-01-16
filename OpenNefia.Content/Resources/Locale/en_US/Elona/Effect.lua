Elona.Effect = {
    Common = {
        ItIsCursed = "It's cursed!",
        CursedLaughter = function(target)
            return ("%s hear%s devils laugh."):format(_.name(target), _.s(target))
        end,
        Resists = function(target)
            return ("%s resist%s."):format(_.name(target), _.s(target))
        end,
        CursedConsumable = function(target)
            return ("%s feel%s grumpy."):format(_.name(target), _.s(target))
        end,
    },

    Heal = {
        Slightly = function(source, target)
            return ("%s %s slightly healed."):format(_.name(target), _.is(target))
        end,
        Normal = function(source, target)
            return ("%s %s healed."):format(_.name(target), _.is(target))
        end,
        Greatly = function(source, target)
            return ("%s %s greatly healed."):format(_.name(target), _.is(target))
        end,
        Completely = function(source, target)
            return ("%s %s completely healed."):format(_.name(target), _.is(target))
        end,
    },

    HealMP = {
        Normal = function(source, target)
            return ("%s mana is restored."):format(_.possessive(target))
        end,
        AbsorbMagic = function(source, target)
            return ("%s absorb%s mana from the air."):format(_.name(target), _.s(target))
        end,
    },

    HealSanity = {
        RainOfSanity = function(source, target)
            return ("%s %s completely sane again."):format(_.name(target), _.is(target))
        end,
    },

    Identify = {
        Fully = function(item)
            return ("The item is fully identified as %s."):format(_.name(item))
        end,
        Partially = function(item)
            return ("The item is half-identified as %s."):format(_.name(item))
        end,
        NeedMorePower = "You need higher identification to gain new knowledge.",
    },

    Curse = {
        Action = function(source, target)
            return ("%s point%s at %s and mutter%s a curse."):format(
                _.name(source),
                _.s(source),
                _.name(target),
                _.s(source)
            )
        end,
        Apply = function(source, target, item)
            return ("%s %s glow%s black."):format(_.possessive(target), _.name(item, true, 1), _.s(item))
        end,
        NoEffect = function(source, target)
            return ("%s prayer nullifies the curse."):format(_.possessive(source))
        end,
    },

    Teleport = {
        Prevented = "Magical field prevents teleportation.",
        General = function(ent)
            return ("Suddenly, %s disappear%s."):format(_.name(ent, true), _.s(ent))
        end,
        DrawShadow = function(ent)
            return ("%s %s drawn."):format(_.name(ent, true), _.is(ent))
        end,
        ShadowStep = function(ent, source, target)
            return ("%s teleport%s toward %s."):format(_.name(source, true), _.s(source), _.basename(target))
        end,
    },

    GainAlly = {
        YoungerSister = "How...! You suddenly get a younger sister!",
        YoungLady = "A young lady falls from the sky.",
        CatSister = "How...! You suddenly get a younger cat sister!",
    },

    Oracle = {
        Cursed = "You hear a sepulchral whisper but the voice is too small to distinguish a word.",
        NoArtifactsYet = "No artifacts have been generated yet.",
    },

    Uncurse = {
        Power = {
            Normal = function(source, target)
                return ("%s equipment are surrounded by a white aura."):format(_.possessive(target))
            end,
            Blessed = function(source, target)
                return ("%s %s surrounded by a holy aura."):format(_.name(target), _.is(target))
            end,
        },
        Apply = {
            Equipment = function(source, target)
                return ("The aura uncurses some of %s equipment."):format(_.his(target))
            end,
            Item = function(source, target)
                return ("The aura uncurses some %s stuff."):format(_.his(target))
            end,
            Resisted = "Several items resist the aura and remain cursed.",
        },
    },

    WallCreation = {
        WallAppears = "A wall appears.",
    },

    DoorCreation = {
        WallsResist = "These walls seem to resist your magic.",
        DoorAppears = "A door appears.",
    },

    WizardsHarvest = {
        FallsDown = function(source, item)
            return ("%s fall%s down!"):format(_.name(item), _.s(item))
        end,
    },

    Restore = {
        Body = {},
        Spirit = {},
    },

    Restore = {
        Body = {
            Apply = function(source, target)
                return ("%s body is restored."):format(_.possessive(target))
            end,
            Blessed = function(source, target)
                return ("In addition, %s body is enchanted."):format(_.his(target))
            end,
            Cursed = function(source, target)
                return ("%s body is damaged."):format(_.possessive(target))
            end,
        },
        Spirit = {
            Apply = function(source, target)
                return ("%s spirit is restored."):format(_.possessive(target))
            end,
            Blessed = function(source, target)
                return ("In addition, %s spirit is enchanted."):format(_.his(target))
            end,
            Cursed = function(source, target)
                return ("%s spirit is damaged."):format(_.possessive(target))
            end,
        },
    },

    Mutation = {
        Apply = function(source, target)
            return ("%s mutate%s."):format(_.name(target), _.s(target))
        end,
        Resist = function(source, target)
            return ("%s resist%s the threat of mutation."):format(_.name(target), _.s(target))
        end,
        Eye = function(source, target)
            return ("%s cast%s an insane glance on %s."):format(_.name(source), _.s(source), _.name(target))
        end,
    },

    CureMutation = {
        Message = function(source, target)
            return ("%s %s now one step closer to %s."):format(_.name(target), _.is(target), _.himself(target))
        end,
    },

    Domination = {
        CannotBeCharmed = function(source, target)
            return ("%s cannot be charmed."):format(_.name(target))
        end,
        DoesNotWorkHere = "The effect doesn't work in this area.",
    },

    Resurrection = {
        Cursed = "Hoards of undead rise from the grave!",

        Prompt = "Resurrect who?",

        Apply = function(source, target)
            return ("%s %s been resurrected!"):format(_.name(target), _.has(target))
        end,
        Fail = function(source, target)
            return ("%s prayer doesn't reach the underworld."):format(_.possessive(source))
        end,

        Dialog = _.quote "Thanks!",
    },

    Sense = {
        Cursed = function(source, target)
            return ("...Huh? %s suffer%s a minor memory defect."):format(_.capitalize(_.name(source)), _.s(source))
        end,

        MagicMap = function(source, target)
            return ("%s sense%s nearby locations."):format(_.name(source), _.s(source))
        end,

        SenseObject = function(source, target)
            return ("%s sense%s nearby objects."):format(_.name(source), _.s(source))
        end,
    },

    FourDimensionalPocket = {
        Summon = "You summon 4 dimensional pocket.",
    },

    Meteor = {
        Falls = "Innumerable meteorites fall all over the area!",
    },

    DrainBlood = {
        Ally = function(source, target)
            return ("%s suck%s %s blood."):format(_.name(source), _.s(source), _.possessive(target))
        end,
        Other = function(source, target)
            return ("%s suck%s %s blood and"):format(_.name(source), _.s(source), _.possessive(target))
        end,
    },

    TouchOfWeakness = {
        Apply = function(source, target)
            return ("%s %s weakened."):format(_.name(target), _.is(target))
        end,
    },

    TouchOfHunger = {
        Apply = function(source, target)
            return ("Suddenly, %s feel%s hungry."):format(_.name(target), _.s(target))
        end,
    },

    ManisDisassembly = {
        Dialog = _.quote "Delete.",
    },
}
