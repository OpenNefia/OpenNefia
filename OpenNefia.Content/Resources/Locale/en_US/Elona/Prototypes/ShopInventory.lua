local function trim_job(name)
    return name -- TODO
end

OpenNefia.Prototypes.Elona.ShopInventory.Elona = {
    Baker = {
        Title = function(name)
            return ("%s the baker"):format(trim_job(name))
        end,
    },
    Blackmarket = {
        Title = function(name)
            return ("%s the blackmarket vendor"):format(trim_job(name))
        end,
        ServantTitle = function(entity)
            return ("%s of blackmarket"):format(_.basename(entity))
        end,
    },
    Blacksmith = {
        Title = function(name)
            return ("%s the blacksmith"):format(trim_job(name))
        end,
        ServantTitle = function(entity)
            return ("%s of armory"):format(_.basename(entity))
        end,
    },
    DyeVendor = {
        Title = function(name)
            return ("%s the dye vendor"):format(trim_job(name))
        end,
    },
    Fence = {
        Title = function(name)
            return ("%s the fence"):format(trim_job(name))
        end,
    },
    Fisher = {
        Title = function(name)
            return ("%s the fisher"):format(trim_job(name))
        end,
    },
    FoodVendor = {
        Title = function(name)
            return ("%s the food vendor"):format(trim_job(name))
        end,
    },
    GeneralVendor = {
        Title = function(name)
            return ("%s the general vendor"):format(trim_job(name))
        end,
        ServantTitle = function(entity)
            return ("%s of general store"):format(_.basename(entity))
        end,
    },
    GoodsVendor = {
        Title = function(name)
            return ("%s the goods vendor"):format(trim_job(name))
        end,
        ServantTitle = function(entity)
            return ("%s of goods store"):format(_.basename(entity))
        end,
    },
    HorseMaster = {
        Title = function(name)
            return ("%s the horse master"):format(trim_job(name))
        end,
    },
    Innkeeper = {
        Title = function(name)
            return ("%s the innkeeper"):format(trim_job(name))
        end,
    },
    MagicVendor = {
        Title = function(name)
            return ("%s the magic vendor"):format(trim_job(name))
        end,
        ServantTitle = function(entity)
            return ("%s of magic store"):format(_.basename(entity))
        end,
    },
    SlaveMaster = "The slave master",
    SouvenirVendor = {
        Title = function(name)
            return ("%s the souvenir vendor"):format(trim_job(name))
        end,
    },
    SpellWriter = {
        Title = function(name)
            return ("%s the spell writer"):format(trim_job(name))
        end,
    },
    StreetVendor = {
        Title = function(name)
            return ("%s the street vendor"):format(trim_job(name))
        end,
    },
    StreetVendor2 = {
        Title = function(name)
            return ("%s the street vendor"):format(trim_job(name))
        end,
    },
    Trader = {
        Title = function(name)
            return ("%s the trader"):format(trim_job(name))
        end,
    },
    Trainer = {
        Title = function(name)
            return ("%s the trainer"):format(trim_job(name))
        end,
    },
    WanderingVendor = {
        Title = function(name)
            return ("%s the wandering vendor"):format(trim_job(name))
        end,
    },
}
