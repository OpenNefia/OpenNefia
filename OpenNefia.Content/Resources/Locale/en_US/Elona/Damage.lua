Elona.Damage = {
    Furthermore = "Futhermore,",
    Attacks = {
        Active = {
            Armed = function(attacker, verb, target, weapon)
                return ("%s %s%s %s and"):format(_.name(attacker), verb, _.s(attacker), _.name(target))
            end,
            Unarmed = function(attacker, verb, target)
                return ("%s %s%s %s and"):format(_.name(attacker), verb, _.s(attacker), _.name(target))
            end,
        },
        Passive = {
            Armed = function(attacker, verb, target, weapon)
                return ("%s %s%s %s with %s %s."):format(
                    _.name(attacker),
                    verb,
                    _.s(attacker),
                    _.name(target),
                    _.his(attacker),
                    _.name(weapon)
                )
            end,
            Unarmed = function(attacker, verb, target)
                return ("%s %s%s %s."):format(_.name(attacker), verb, _.s(attacker), _.name(target))
            end,
        },
    },
    Levels = {
        Scratch = function(entity, attacker)
            return ("make%s a scratch."):format(_.s(attacker))
        end,
        Slightly = function(entity, attacker)
            return ("slightly wound%s %s."):format(_.s(attacker), _.him(entity))
        end,
        Moderately = function(entity, attacker)
            return ("moderately wound%s %s."):format(_.s(attacker), _.him(entity))
        end,
        Severely = function(entity, attacker)
            return ("severely wound%s %s."):format(_.s(attacker), _.him(entity))
        end,
        Critically = function(entity, attacker)
            return ("critically wound%s %s!"):format(_.s(attacker), _.him(entity))
        end,
    },
    Reactions = {
        Screams = function(entity)
            return ("%s scream%s."):format(_.name(entity), _.s(entity))
        end,
        WrithesInPain = function(entity)
            return ("%s writhe%s in pain."):format(_.name(entity), _.s(entity))
        end,
        IsSeverelyHurt = function(entity)
            return ("%s %s severely hurt!"):format(_.name(entity), _.is(entity))
        end,
        IsHealed = function(entity)
            return ("%s %s healed."):format(_.name(entity), _.is(entity))
        end,
    },
    Wounded = function(target, attacker)
        return ("%s %s wounded."):format(_.name(target), _.is(target))
    end,
    Killed = {
        Active = function(target, attacker)
            return ("kill%s %s."):format(_.s(attacker), _.him(target))
        end,
        Passive = function(target, attacker)
            return ("%s %s killed."):format(_.name(target), _.is(target))
        end,
    },
    YouFeelSad = "You feel sad for a moment.",
    MagicReaction = {
        Hurts = function(entity)
            return ("Magic reaction hurts %s!"):format(_.name(entity))
        end,
    },
    RunsAway = function(entity)
        return ("%s run%s away in terror."):format(_.name(entity), _.s(entity))
    end,
    SleepIsDisturbed = function(entity)
        return ("%s sleep %s disturbed."):format(_.possessive(entity), _.is(entity))
    end,
}
