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
        Baker = function(_1)
            return ("%sthe baker"):format(trim_job(_1))
        end,
        Blackmarket = function(_1)
            return ("%sthe blackmarket vendor"):format(trim_job(_1))
        end,
        Blacksmith = function(_1)
            return ("%sthe blacksmith"):format(trim_job(_1))
        end,
        DyeVendor = function(_1)
            return ("%sthe dye vendor"):format(trim_job(_1))
        end,
        Fanatic = { "Opatos Fanatic", "Mani Fanatic", "Ehekatl Fanatic" },
        Fence = function(_1)
            return ("%sthe fence"):format(trim_job(_1))
        end,
        Fisher = function(_1)
            return ("%sthe fisher"):format(trim_job(_1))
        end,
        FoodVendor = function(_1)
            return ("%sthe food vendor"):format(trim_job(_1))
        end,
        GeneralVendor = function(_1)
            return ("%sthe general vendor"):format(trim_job(_1))
        end,
        GoodsVendor = function(_1)
            return ("%sthe goods vendor"):format(trim_job(_1))
        end,
        HorseMaster = function(_1)
            return ("%sthe horse master"):format(trim_job(_1))
        end,
        Innkeeper = function(_1)
            return ("%sthe innkeeper"):format(trim_job(_1))
        end,
        MagicVendor = function(_1)
            return ("%sthe magic vendor"):format(trim_job(_1))
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
        SouvenirVendor = function(_1)
            return ("%sthe souvenir vendor"):format(trim_job(_1))
        end,
        SpellWriter = function(_1)
            return ("%sthe spell writer"):format(trim_job(_1))
        end,
        StreetVendor = function(_1)
            return ("%sthe street vendor"):format(trim_job(_1))
        end,
        StreetVendor2 = function(_1)
            return ("%sthe street vendor"):format(trim_job(_1))
        end,
        Trader = function(_1)
            return ("%sthe trader"):format(trim_job(_1))
        end,
        Trainer = function(_1)
            return ("%sthe trainer"):format(trim_job(_1))
        end,
        WanderingVendor = function(_1)
            return ("%sthe wandering vendor"):format(trim_job(_1))
        end,
    },
}
