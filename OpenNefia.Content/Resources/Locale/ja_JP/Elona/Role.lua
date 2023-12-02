local function trim_job(name)
    -- TODO
    return name
end

Elona.Role = {
    Names = {
        Alien = {
            AlienKid = "alien kid",
            Child = "child",
            ChildOf = function(_1)
                return ("child of %s"):format(_1)
            end,
        },
        Fanatic = { "Opatos Fanatic", "Mani Fanatic", "Ehekatl Fanatic" },
        HorseMaster = function(_1)
            return ("%s the horse master"):format(trim_job(_1))
        end,
        OfDerphy = function(_1)
            return ("%s of Derphy"):format(_1)
        end,
        OfLumiest = function(_1)
            return ("%s of Lumiest"):format(_1)
        end,
        OfNoyel = function(_1)
            return ("%s of Noyel"):format(_1)
        end,
        OfPalmia = function(_1)
            return ("%s of Palmia city"):format(_1)
        end,
        OfPortKapul = function(_1)
            return ("%s of Port Kapul"):format(_1)
        end,
        OfVernis = function(_1)
            return ("%s of Vernis"):format(_1)
        end,
        OfYowyn = function(_1)
            return ("%s of Yowyn"):format(_1)
        end,
        Shade = "shade",
        SlaveMaster = "The slave master",
        SpellWriter = function(_1)
            return ("%s the spell writer"):format(trim_job(_1))
        end,
        Trainer = function(_1)
            return ("%s the trainer"):format(trim_job(_1))
        end,
    },
}
