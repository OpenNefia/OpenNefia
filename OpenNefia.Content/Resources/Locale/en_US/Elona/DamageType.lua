Elona.DamageType = {
    Default = {
        Damage = function(entity)
            return ("%s %s wounded."):format(_.name(entity), _.is(entity))
        end,
        Death = {
            Active = function(entity, attacker, direct)
                return ("kill%s %s."):format(direct and _.s(attacker) or "s", _.him(entity))
            end,
            Passive = function(entity)
                return ("%s %s killed."):format(_.name(entity), _.is(entity))
            end,
        },
    },
    Combat = {
        DeathCause = function(attacker)
            return ("was killed by %s"):format(_.basename(attacker))
        end,
        Killed = {
            Active = function(entity, attacker, direct)
                return ("kill%s %s."):format(direct and _.s(attacker) or "s", _.him(entity))
            end,
            Passive = function(entity)
                return ("%s %s slain."):format(_.name(entity), _.is(entity))
            end,
        },
        Minced = {
            Active = function(entity, attacker)
                return ("mince%s %s."):format(direct and _.s(attacker) or "s", _.him(entity))
            end,
            Passive = function(entity)
                return ("%s %s minced."):format(_.name(entity), _.is(entity))
            end,
        },
        TransformedIntoMeat = {
            Active = function(entity, attacker, direct)
                return ("transform%s %s into several pieces of meat."):format(
                    direct and _.s(attacker) or "s",
                    _.him(entity)
                )
            end,
            Passive = function(entity)
                return ("%s %s transformed into several pieces of meat."):format(_.name(entity), _.is(entity))
            end,
        },
        Destroyed = {
            Active = function(entity, attacker, direct)
                return ("destroy%s %s."):format(direct and _.s(attacker) or "s", _.him(entity))
            end,
            Passive = function(entity)
                return ("%s %s killed."):format(_.name(entity), _.is(entity))
            end,
        },
    },
    Trap = {
        DeathCause = "got caught in a trap and died",
        Message = function(entity)
            return ("%s %s caught in a trap and die%s."):format(_.name(entity), _.is(entity), _.s(entity))
        end,
    },
    MagicReaction = {
        DeathCause = "was completely wiped by magic reaction",
        Message = function(entity)
            return ("%s die%s from over-casting."):format(_.name(entity), _.s(entity))
        end,
    },
    Starvation = {
        DeathCause = "was starved to death",
        Message = function(entity)
            return ("%s %s starved to death."):format(_.name(entity), _.is(entity))
        end,
    },
    Poison = {
        DeathCause = "miserably died from poison",
        Message = function(entity)
            return ("%s %s killed with poison."):format(_.name(entity), _.is(entity))
        end,
    },
    Curse = {
        DeathCause = "died from curse",
        Message = function(entity)
            return ("%s die%s from curse."):format(_.name(entity), _.s(entity))
        end,
    },
    Burden = {
        Backpack = "backpack",
        DeathCause = function(itemName)
            return ("was squashed by %s"):format(itemName)
        end,
        Message = function(entity, itemName)
            return ("%s %s squashed by %s."):format(_.name(entity), _.is(entity), itemName)
        end,
    },
    Stairs = {
        DeathCause = "tumbled from stairs and died",
        Message = function(entity)
            return ("%s tumble%s from stairs and die%s."):format(_.name(entity), _.s(entity), _.s(entity))
        end,
    },
    Performance = {
        DeathCause = "was killed by an audience",
        Message = function(entity)
            return ("%s %s killed by an audience."):format(_.name(entity), _.is(entity))
        end,
    },
    Burning = {
        DeathCause = "was burnt and turned into ash",
        Message = function(entity)
            return ("%s %s burnt and turned into ash."):format(_.name(entity), _.is(entity))
        end,
    },
    UnseenHand = {
        DeathCause = "got assassinated by the unseen hand",
        Message = function(entity)
            return ("%s %s assassinated by the unseen hand."):format(_.name(entity), _.is(entity))
        end,
    },
    FoodPoisoning = {
        DeathCause = "got killed by food poisoning",
        Message = function(entity)
            return ("%s %s killed by food poisoning."):format(_.name(entity), _.is(entity))
        end,
    },
    Bleeding = {
        DeathCause = "died from loss of blood",
        Message = function(entity)
            return ("%s die%s from loss of blood."):format(_.name(entity), _.s(entity))
        end,
    },
    EtherDisease = {
        DeathCause = "died of the Ether disease",
        Message = function(entity)
            return ("%s die%s of the Ether disease."):format(_.name(entity), _.s(entity))
        end,
    },
    Acid = {
        DeathCause = "melted down",
        Message = function(entity)
            return ("%s melt%s down."):format(_.name(entity), _.s(entity))
        end,
    },
    Suicide = {
        DeathCause = "committed suicide",
        Message = function(entity)
            return ("%s shatter%s."):format(_.name(entity), _.s(entity))
        end,
    },
    Nuke = {
        DeathCause = "was killed by an atomic bomb",
        Message = function(entity)
            return ("%s %s turned into atoms."):format(_.name(entity), _.is(entity))
        end,
    },
    IronMaiden = {
        DeathCause = "stepped in an iron maiden and died",
        Message = function(entity)
            return ("%s step%s into an iron maiden and die%s."):format(_.name(entity), _.s(entity), _.s(entity))
        end,
    },
    Guillotine = {
        DeathCause = "was guillotined",
        Message = function(entity)
            return ("%s %s guillotined and die%s."):format(_.name(entity), _.is(entity), _.s(entity))
        end,
    },
    Hanging = {
        DeathCause = "commited suicide by hanging",
        Message = function(entity)
            return ("%s hang%s %sself."):format(_.name(entity), _.s(entity), _.his(entity))
        end,
    },
    Mochi = {
        DeathCause = "ate mochi and died",
        Message = function(entity)
            return ("%s choke%s on mochi and die."):format(_.name(entity), _.s(entity))
        end,
    },
}
