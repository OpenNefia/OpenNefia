Elona.Damage = {
    Furthermore = "此外，",
    Attacks = {
        Active = {
            Armed = function(attacker, verb, target, weapon)
                return ("%s %s%s %s，而且"):format(_.name(attacker), verb, _.s(attacker), _.name(target))
            end,
            Unarmed = function(attacker, verb, target)
                return ("%s %s%s %s，而且"):format(_.name(attacker), verb, _.s(attacker), _.name(target))
            end,
        },
        Passive = {
            Armed = function(attacker, verb, target, weapon)
                return ("%s %s%s %s，使用 %s %s。"):format(
                    _.name(attacker),
                    verb,
                    _.s(attacker),
                    _.name(target),
                    _.his(attacker),
                    _.name(weapon)
                )
            end,
            Unarmed = function(attacker, verb, target,verb_a)--火龙 被 爪击 你
                --return ("%s %s%s %s。"):format(_.name(attacker), verb, _.s(attacker), _.name(target))
                --你被爪击 火龙
                return ("%s%s %s%s %s攻击。"):format(_.name(target),verb,_.name(attacker), _.s(attacker),verb_a)
            end,
        },
    },
    Levels = {
        Scratch = function(entity, attacker, direct)
            return ("擦伤%s。"):format(direct and _.s(attacker) or "s")
        end,
        Slightly = function(entity, attacker, direct)
            return ("轻微地伤害%s %s。"):format(direct and _.s(attacker) or "s", _.him(entity))
        end,
        Moderately = function(entity, attacker, direct)
            return ("中度伤害%s %s。"):format(direct and _.s(attacker) or "s", _.him(entity))
        end,
        Severely = function(entity, attacker, direct)
            return ("严重伤害%s %s。"):format(direct and _.s(attacker) or "s", _.him(entity))
        end,
        Critically = function(entity, attacker, direct)
            return ("重伤%s %s!"):format(direct and _.s(attacker) or "s", _.him(entity))
        end,
    },
    Reactions = {
        Screams = function(entity)
            return ("%s 尖叫%s。"):format(_.name(entity), _.s(entity))
        end,
        WrithesInPain = function(entity)
            return ("%s 扭动%s因痛苦而。"):format(_.name(entity), _.s(entity))
        end,
        IsSeverelyHurt = function(entity)
            return ("%s 受伤%s严重！"):format(_.name(entity), _.is(entity))
        end,
        IsHealed = function(entity)
            return ("%s %s痊愈。"):format(_.name(entity), _.is(entity))
        end,
    },
    Wounded = function(target, attacker)
        return ("%s %s受伤。"):format(_.name(target), _.is(target))
    end,
    Killed = {
        Active = function(target, attacker, direct) -- TODO move?
            return ("%s 杀死%s。"):format(direct and _.s(attacker) or "s", _.him(target))
        end,
        Passive = function(target, attacker)
            return ("%s %s被杀。"):format(_.name(target), _.is(target))
        end,
    },
    YouFeelSad = "你感到一阵悲伤。",
    MagicReaction = {
        Hurts = function(entity)
            return ("魔法反应伤害了 %s!"):format(_.name(entity))
        end,
    },
    RunsAway = function(entity)
        return ("%s 惊恐地逃走了。"):format(_.name(entity), _.s(entity))
    end,
    SleepIsDisturbed = function(entity)
        return ("%s 睡眠 %s被打扰。"):format(_.possessive(entity), _.is(entity))
    end,
}
