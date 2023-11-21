OpenNefia.Prototypes.Elona.ShopInventory.Elona = {
    Baker = {
        Title = function(name)
            return ("パン屋の%s"):format(name)
        end,
    },
    Blackmarket = {
        Title = function(name)
            return ("ブラックマーケットの%s"):format(name)
        end,
    },
    Blacksmith = {
        Title = function(name)
            return ("武具店の%s"):format(name)
        end,
        ServantTitle = function(entity)
            return ("武具店の%s"):format(_.basename(entity))
        end,
    },
    DyeVendor = {
        Title = function(name)
            return ("染色店の%s"):format(name)
        end,
    },
    Fence = {
        Title = function(name)
            return ("盗賊店の%s"):format(name)
        end,
    },
    Fisher = {
        Title = function(name)
            return ("釣具店の%s"):format(name)
        end,
    },
    FoodVendor = {
        Title = function(name)
            return ("食品店%s"):format(name)
        end,
    },
    GeneralVendor = {
        Title = function(name)
            return ("雑貨屋の%s"):format(name)
        end,
        ServantTitle = function(entity)
            return ("雑貨屋の%s"):format(_.basename(entity))
        end,
    },
    GoodsVendor = {
        Title = function(name)
            return ("何でも屋の%s"):format(name)
        end,
        ServantTitle = function(entity)
            return ("何でも屋の%s"):format(_.basename(entity))
        end,
    },
    HorseMaster = {
        Title = function(name)
            return ("馬屋の%s"):format(name)
        end,
    },
    Innkeeper = {
        Title = function(name)
            return ("宿屋の%s"):format(name)
        end,
    },
    MagicVendor = {
        Title = function(name)
            return ("魔法店の%s"):format(name)
        end,
        ServantTitle = function(entity)
            return ("魔法店の%s"):format(_.basename(entity))
        end,
    },
    SlaveMaster = {
        Title = "謎の奴隷商人",
    },
    SouvenirVendor = {
        Title = function(name)
            return ("おみやげ屋の%s"):format(name)
        end,
    },
    SpellWriter = {
        Title = function(name)
            return ("魔法書作家の%s"):format(name)
        end,
    },
    StreetVendor = {
        Title = function(name)
            return ("屋台商人の%s"):format(name)
        end,
    },
    StreetVendor2 = {
        Title = function(name)
            return ("屋台商人屋の%s"):format(name)
        end,
    },
    Trader = {
        Title = function(name)
            return ("交易店の%s"):format(name)
        end,
    },
    Trainer = {
        Title = function(name)
            return ("ギルドの%s"):format(name)
        end,
    },
    WanderingVendor = {
        Title = function(name)
            return ("行商人の%s"):format(name)
        end,
    },
}
