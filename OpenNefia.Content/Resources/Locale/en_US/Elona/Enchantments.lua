Elona.Enchantment = {
    PowerUnit = "#",

    Item = {
        ModifyAttribute = {
            Equipment = {
                Increases = function(item, skillName, power)
                    return ("%s increases your %s by %s."):format(_.he(item), skillName, power)
                end,
                Decreases = function(item, skillName, power)
                    return ("%s decreases your %s by %s."):format(_.he(item), skillName, power)
                end,
            },
            Food = {
                Increases = function(item, skillName, power)
                    return ("%s has essential nutrients to enhance your %s."):format(_.he(item), skillName)
                end,
                Decreases = function(item, skillName, power)
                    return ("%s has which deteriorates your %s."):format(_.he(item), skillName)
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
