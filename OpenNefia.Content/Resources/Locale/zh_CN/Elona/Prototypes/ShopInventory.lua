OpenNefia.Prototypes.Elona.ShopInventory.Elona = {
    Baker = {
        Title = function(name)
            return ("面包店的%s"):format(name)
        end,
    },
    Blackmarket = {
        Title = function(name)
            return ("黑市的%s"):format(name)
        end,
    },
    Blacksmith = {
        Title = function(name)
            return ("武具店的%s"):format(name)
        end,
        ServantTitle = function(entity)
            return ("武具店的%s"):format(_.basename(entity))
        end,
    },
    DyeVendor = {
        Title = function(name)
            return ("染色店的%s"):format(name)
        end,
    },
    Fence = {
        Title = function(name)
            return ("盗贼店的%s"):format(name)
        end,
    },
    Fisher = {
        Title = function(name)
            return ("渔具店的%s"):format(name)
        end,
    },
    FoodVendor = {
        Title = function(name)
            return ("食品店的%s"):format(name)
        end,
    },
    GeneralVendor = {
        Title = function(name)
            return ("杂货店的%s"):format(name)
        end,
        ServantTitle = function(entity)
            return ("杂货店的%s"):format(_.basename(entity))
        end,
    },
    GoodsVendor = {
        Title = function(name)
            return ("杂货店的%s"):format(name)
        end,
        ServantTitle = function(entity)
            return ("杂货店的%s"):format(_.basename(entity))
        end,
    },
    HorseMaster = {
        Title = function(name)
            return ("马店的%s"):format(name)
        end,
    },
    Innkeeper = {
        Title = function(name)
            return ("旅馆的%s"):format(name)
        end,
    },
    MagicVendor = {
        Title = function(name)
            return ("魔法店的%s"):format(name)
        end,
        ServantTitle = function(entity)
            return ("魔法店的%s"):format(_.basename(entity))
        end,
    },
    SlaveMaster = {
        Title = "神秘的奴隶商人",
    },
    SouvenirVendor = {
        Title = function(name)
            return ("纪念品店的%s"):format(name)
        end,
    },
    SpellWriter = {
        Title = function(name)
            return ("魔法书作家的%s"):format(name)
        end,
    },
    StreetVendor = {
        Title = function(name)
            return ("摊贩的%s"):format(name)
        end,
    },
    StreetVendor2 = {
        Title = function(name)
            return ("摊贩屋的%s"):format(name)
        end,
    },
    Trader = {
        Title = function(name)
            return ("交易店的%s"):format(name)
        end,
    },
    Trainer = {
        Title = function(name)
            return ("公会的%s"):format(name)
        end,
    },
    WanderingVendor = {
        Title = function(name)
            return ("流浪商人的%s"):format(name)
        end,
    },
}