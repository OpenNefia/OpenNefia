OpenNefia.Prototypes.Elona.Element.Elona = {
    Chaos = {
        Name = "chaos",
        ShortName = "Ch",
        Description = "Resistance to chaos.",
        Ego = "chaotic",
        Wounded = function(entity)
            return ("%s %s hurt by chaotic force."):format(_.name(entity), _.is(entity))
        end,
        Resist = {
            Gain = function(entity)
                return ("Suddenly, %s understand%s chaos."):format(_.name(entity), _.s(entity))
            end,
            Lose = function(entity)
                return ("%s no longer understand%s chaos."):format(_.name(entity), _.s(entity))
            end,
        },
        Killed = {
            Active = function(entity, attacker, direct)
                return ("let%s the chaos consume %s."):format(direct and _.s(attacker) or "s", _.him(entity))
            end,
            Passive = function(entity)
                return ("%s %s drawn into a chaotic vortex."):format(_.name(entity), _.is(entity))
            end,
        },
    },
    Cold = {
        Name = "cold",
        ShortName = "Co",
        Description = "Resistance to cold.",
        Ego = "icy",
        Wounded = function(entity)
            return ("%s %s frozen."):format(_.name(entity), _.is(entity))
        end,
        Resist = {
            Gain = function(entity)
                return ("Suddenly, %s feel%s very cool."):format(_.name(entity), _.s(entity))
            end,
            Lose = function(entity)
                return ("%s shiver%s."):format(_.name(entity), _.s(entity))
            end,
        },
        Killed = {
            Active = function(entity, attacker, direct)
                return ("transform%s %s into an ice sculpture."):format(direct and _.s(attacker) or "s", _.him(entity))
            end,
            Passive = function(entity)
                return ("%s %s frozen and turn%s into an ice sculpture."):format(
                    _.name(entity),
                    _.is(entity),
                    _.s(entity)
                )
            end,
        },
    },
    Cut = {
        Name = "cut",
        Description = "Resistance to cut.",
        Ego = "cut",
        Wounded = function(entity)
            return ("%s get%s a cut."):format(_.name(entity), _.s(entity))
        end,
        Killed = {
            Active = function(entity, attacker, direct)
                return ("cut%s %s into thin strips."):format(direct and _.s(attacker) or "s", _.him(entity))
            end,
            Passive = function(entity)
                return ("%s %s cut into thin strips."):format(_.name(entity), _.is(entity))
            end,
        },
    },
    Darkness = {
        Name = "darkness",
        ShortName = "Da",
        Description = "Resistance to darkness.",
        Ego = "gloomy",
        Wounded = function(entity)
            return ("%s %s struck by dark force."):format(_.name(entity), _.is(entity))
        end,
        Resist = {
            Gain = function(entity)
                return ("%s no longer fear%s darkness."):format(_.name(entity), _.s(entity))
            end,
            Lose = function(entity)
                return ("Suddenly, %s fear%s darkness."):format(_.name(entity), _.s(entity))
            end,
        },
        Killed = {
            Active = function(entity, attacker, direct)
                return ("let%s the depths swallow %s."):format(direct and _.s(attacker) or "s", _.him(entity))
            end,
            Passive = function(entity)
                return ("%s %s consumed by darkness."):format(_.name(entity), _.is(entity))
            end,
        },
    },
    Fire = {
        Name = "fire",
        ShortName = "Fi",
        Description = "Resistance to fire.",
        Ego = "burning",
        Wounded = function(entity)
            return ("%s %s burnt."):format(_.name(entity), _.is(entity))
        end,
        Resist = {
            Gain = function(entity)
                return ("Suddenly, %s feel%s very hot."):format(_.name(entity), _.s(entity))
            end,
            Lose = function(entity)
                return ("%s sweat%s."):format(_.name(entity), _.s(entity))
            end,
        },
        Killed = {
            Active = function(entity, attacker, direct)
                return ("burn%s %s to death."):format(direct and _.s(attacker) or "s", _.him(entity))
            end,
            Passive = function(entity)
                return ("%s %s burnt to ashes."):format(_.name(entity), _.is(entity))
            end,
        },
    },
    Lightning = {
        Name = "lightning",
        ShortName = "Li",
        Description = "Resistance to lightning.",
        Ego = "electric",
        Wounded = function(entity)
            return ("%s %s shocked."):format(_.name(entity), _.is(entity))
        end,
        Resist = {
            Gain = function(entity)
                return ("%s %s struck by an electric shock."):format(_.name(entity), _.is(entity))
            end,
            Lose = function(entity)
                return ("%s %s shocked."):format(_.name(entity), _.is(entity))
            end,
        },
        Killed = {
            Active = function(entity, attacker, direct)
                return ("electrocute%s %s to death."):format(direct and _.s(attacker) or "s", _.him(entity))
            end,
            Passive = function(entity)
                return ("%s %s struck by lightning and die%s."):format(_.name(entity), _.is(entity), _.s(entity))
            end,
        },
    },
    Magic = {
        Name = "magic",
        ShortName = "Ma",
        Description = "Resistance to magic.",
        Resist = {
            Gain = function(entity)
                return ("%s%s body is covered by a magical aura."):format(_.name(entity), _.possessive(entity))
            end,
            Lose = function(entity)
                return ("The magical aura disappears from %s%s body."):format(_.name(entity), _.possessive(entity))
            end,
        },
    },
    Mind = {
        Name = "mind",
        ShortName = "Mi",
        Description = "Resistance to mind.",
        Ego = "psychic",
        Wounded = function(entity)
            return ("%s suffer%s a splitting headache."):format(_.name(entity), _.s(entity))
        end,
        Resist = {
            Gain = function(entity)
                return ("Suddenly, %s%s mind becomes very clear."):format(_.name(entity), _.possessive(entity))
            end,
            Lose = function(entity)
                return ("%s%s mind becomes slippery."):format(_.name(entity), _.possessive(entity))
            end,
        },
        Killed = {
            Active = function(entity, attacker, direct)
                return ("completely disable%s %s."):format(direct and _.s(attacker) or "s", _.him(entity))
            end,
            Passive = function(entity)
                return ("%s lose%s %s mind and commit%s a suicide."):format(
                    _.name(entity),
                    _.s(entity),
                    _.his(entity),
                    _.s(entity)
                )
            end,
        },
    },
    Nerve = {
        Name = "nerve",
        ShortName = "Nr",
        Description = "Resistance to nerve.",
        Ego = "numb",
        Wounded = function(entity)
            return ("%s%s nerves are hurt."):format(_.name(entity), _.possessive(entity))
        end,
        Resist = {
            Gain = function(entity)
                return ("%s%s nerve is sharpened."):format(_.name(entity), _.possessive(entity))
            end,
            Lose = function(entity)
                return ("%s become%s dull."):format(_.name(entity), _.s(entity))
            end,
        },
        Killed = {
            Active = function(entity, attacker, direct)
                return ("destroy%s %s nerves."):format(direct and _.s(attacker) or "s", _.his(entity))
            end,
            Passive = function(entity)
                return ("%s die%s from neurofibroma."):format(_.name(entity), _.s(entity))
            end,
        },
    },
    Nether = {
        Name = "nether",
        ShortName = "Mt",
        Description = "Resistance to nether.",
        Ego = "infernal",
        Wounded = function(entity)
            return ("%s %s chilled by infernal squall."):format(_.name(entity), _.is(entity))
        end,
        Resist = {
            Gain = function(entity)
                return ("%s %s no longer afraid of hell."):format(_.name(entity), _.is(entity))
            end,
            Lose = function(entity)
                return ("%s %s afraid of hell."):format(_.name(entity), _.is(entity))
            end,
        },
        Killed = {
            Active = function(entity, attacker, direct)
                return ("entrap%s %s into the inferno."):format(direct and _.s(attacker) or "s", _.him(entity))
            end,
            Passive = function(entity)
                return ("%s go%s to hell."):format(_.name(entity), _.s(entity, true))
            end,
        },
    },
    Poison = {
        Name = "poison",
        ShortName = "Po",
        Description = "Resistance to poison.",
        Ego = "poisonous",
        Wounded = function(entity)
            return ("%s suffer%s from venom."):format(_.name(entity), _.s(entity))
        end,
        Resist = {
            Gain = function(entity)
                return ("%s now %s antibodies to poisons."):format(_.name(entity), _.has(entity))
            end,
            Lose = function(entity)
                return ("%s lose%s antibodies to poisons."):format(_.name(entity), _.s(entity))
            end,
        },
        Killed = {
            Active = function(entity, attacker, direct)
                return ("kill%s %s with poison."):format(direct and _.s(attacker) or "s", _.him(entity))
            end,
            Passive = function(entity)
                return ("%s %s poisoned to death."):format(_.name(entity), _.is(entity))
            end,
        },
    },
    Sound = {
        Name = "sound",
        ShortName = "So",
        Description = "Resistance to sound.",
        Ego = "shivering",
        Wounded = function(entity)
            return ("%s %s shocked by a shrill sound."):format(_.name(entity), _.is(entity))
        end,
        Resist = {
            Gain = function(entity)
                return ("%s%s eardrums get thick."):format(_.name(entity), _.possessive(entity))
            end,
            Lose = function(entity)
                return ("%s become%s very sensitive to noises."):format(_.name(entity), _.s(entity))
            end,
        },
        Killed = {
            Active = function(entity, attacker, direct)
                return ("shatter%s %s to atoms."):format(direct and _.s(attacker) or "s", _.him(entity))
            end,
            Passive = function(entity)
                return ("%s resonate%s and break%s up."):format(_.name(entity), _.s(entity), _.s(entity))
            end,
        },
    },
    Ether = {
        Ego = "ether",
    },
    Acid = {
        Wounded = function(entity)
            return ("%s %s burnt by acid."):format(_.name(entity), _.is(entity))
        end,
        Killed = {
            Active = function(entity, attacker, direct)
                return ("melt%s %s away."):format(direct and _.s(attacker) or "s", _.him(entity))
            end,
            Passive = function(entity)
                return ("%s melt%s."):format(_.name(entity), _.s(entity))
            end,
        },
    },
    Rotten = {
        Ego = "rotten",
    },
    Hunger = {
        Ego = "starving",
    },
    Fear = {
        Ego = "fearful",
    },
    Soft = {
        Ego = "silky",
    },
    Vorpal = {
        Ego = "vorpal",
    },
}
