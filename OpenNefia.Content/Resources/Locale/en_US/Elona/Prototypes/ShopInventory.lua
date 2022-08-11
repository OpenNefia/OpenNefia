local function trim_job(name)
    return name -- TODO
end

OpenNefia.Prototypes.Elona.ShopInventory.Elona = {
    Baker = function(name)
        return ("%sthe baker"):format(trim_job(name))
    end,
    Blackmarket = function(name)
        return ("%sthe blackmarket vendor"):format(trim_job(name))
    end,
    Blacksmith = function(name)
        return ("%sthe blacksmith"):format(trim_job(name))
    end,
    DyeVendor = function(name)
        return ("%sthe dye vendor"):format(trim_job(name))
    end,
    Fence = function(name)
        return ("%sthe fence"):format(trim_job(name))
    end,
    Fisher = function(name)
        return ("%sthe fisher"):format(trim_job(name))
    end,
    FoodVendor = function(name)
        return ("%sthe food vendor"):format(trim_job(name))
    end,
    GeneralVendor = function(name)
        return ("%sthe general vendor"):format(trim_job(name))
    end,
    GoodsVendor = function(name)
        return ("%sthe goods vendor"):format(trim_job(name))
    end,
    HorseMaster = function(name)
        return ("%sthe horse master"):format(trim_job(name))
    end,
    Innkeeper = function(name)
        return ("%sthe innkeeper"):format(trim_job(name))
    end,
    MagicVendor = function(name)
        return ("%sthe magic vendor"):format(trim_job(name))
    end,
    SlaveMaster = "The slave master",
    SouvenirVendor = function(name)
        return ("%sthe souvenir vendor"):format(trim_job(name))
    end,
    SpellWriter = function(name)
        return ("%sthe spell writer"):format(trim_job(name))
    end,
    StreetVendor = function(name)
        return ("%sthe street vendor"):format(trim_job(name))
    end,
    StreetVendor2 = function(name)
        return ("%sthe street vendor"):format(trim_job(name))
    end,
    Trader = function(name)
        return ("%sthe trader"):format(trim_job(name))
    end,
    Trainer = function(name)
        return ("%sthe trainer"):format(trim_job(name))
    end,
    WanderingVendor = function(name)
        return ("%sthe wandering vendor"):format(trim_job(name))
    end,
}
