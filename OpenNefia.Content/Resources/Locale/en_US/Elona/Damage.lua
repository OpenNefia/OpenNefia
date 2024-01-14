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
        Scratch = function(entity, attacker, direct)
            return ("make%s a scratch."):format(direct and _.s(attacker) or "s")
        end,
        Slightly = function(entity, attacker, direct)
            return ("slightly wound%s %s."):format(direct and _.s(attacker) or "s", _.him(entity))
        end,
        Moderately = function(entity, attacker, direct)
            return ("moderately wound%s %s."):format(direct and _.s(attacker) or "s", _.him(entity))
        end,
        Severely = function(entity, attacker, direct)
            return ("severely wound%s %s."):format(direct and _.s(attacker) or "s", _.him(entity))
        end,
        Critically = function(entity, attacker, direct)
            return ("critically wound%s %s!"):format(direct and _.s(attacker) or "s", _.him(entity))
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
        Active = function(target, attacker, direct) -- TODO move?
            return ("kill%s %s."):format(direct and _.s(attacker) or "s", _.him(target))
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
