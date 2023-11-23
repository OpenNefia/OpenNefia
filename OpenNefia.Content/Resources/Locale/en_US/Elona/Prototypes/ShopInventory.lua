local function trim_job(name)
    return name -- TODO
end

OpenNefia.Prototypes.Elona.ShopInventory.Elona = {
    Baker = {
        Title = function(name)
            return ("%sthe baker"):format(trim_job(name))
        end,
    },
    Blackmarket = {
        Title = function(name)
            return ("%sthe blackmarket vendor"):format(trim_job(name))
        end,
        ServantTitle = function(entity)
            return ("%s of blackmarket"):format(_.basename(entity))
        end,
    },
    Blacksmith = {
        Title = function(name)
            return ("%sthe blacksmith"):format(trim_job(name))
        end,
        ServantTitle = function(entity)
            return ("%s of armory"):format(_.basename(entity))
        end,
    },
    DyeVendor = {
        Title = function(name)
            return ("%sthe dye vendor"):format(trim_job(name))
        end,
    },
    Fence = {
        Title = function(name)
            return ("%sthe fence"):format(trim_job(name))
        end,
    },
    Fisher = {
        Title = function(name)
            return ("%sthe fisher"):format(trim_job(name))
        end,
    },
    FoodVendor = {
        Title = function(name)
            return ("%sthe food vendor"):format(trim_job(name))
        end,
    },
    GeneralVendor = {
        Title = function(name)
            return ("%sthe general vendor"):format(trim_job(name))
        end,
        ServantTitle = function(entity)
            return ("%s of general store"):format(_.basename(entity))
        end,
    },
    GoodsVendor = {
        Title = function(name)
            return ("%sthe goods vendor"):format(trim_job(name))
        end,
        ServantTitle = function(entity)
            return ("%s of goods store"):format(_.basename(entity))
        end,
    },
    HorseMaster = {
        Title = function(name)
            return ("%sthe horse master"):format(trim_job(name))
        end,
    },
    Innkeeper = {
        Title = function(name)
            return ("%sthe innkeeper"):format(trim_job(name))
        end,
    },
    MagicVendor = {
        Title = function(name)
            return ("%sthe magic vendor"):format(trim_job(name))
        end,
        ServantTitle = function(entity)
            return ("%s of magic store"):format(_.basename(entity))
        end,
    },
    SlaveMaster = "The slave master",
    SouvenirVendor = {
        Title = function(name)
            return ("%sthe souvenir vendor"):format(trim_job(name))
        end,
    },
    SpellWriter = {
        Title = function(name)
            return ("%sthe spell writer"):format(trim_job(name))
        end,
    },
    StreetVendor = {
        Title = function(name)
            return ("%sthe street vendor"):format(trim_job(name))
        end,
    },
    StreetVendor2 = {
        Title = function(name)
            return ("%sthe street vendor"):format(trim_job(name))
        end,
    },
    Trader = {
        Title = function(name)
            return ("%sthe trader"):format(trim_job(name))
        end,
    },
    Trainer = {
        Title = function(name)
            return ("%sthe trainer"):format(trim_job(name))
        end,
    },
    WanderingVendor = {
        Title = function(name)
            return ("%sthe wandering vendor"):format(trim_job(name))
        end,
    },
}
