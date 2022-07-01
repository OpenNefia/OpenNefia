Elona.Role = {
    Names = {
        Alien = {
            AlienKid = "エイリアンの子供",
            Child = "の子供",
            ChildOf = function(name)
                return ("%sの子供"):format(name)
            end,
        },
        Baker = function(name)
            return ("パン屋の%s"):format(name)
        end,
        Blackmarket = function(name)
            return ("ブラックマーケットの%s"):format(name)
        end,
        Blacksmith = function(name)
            return ("武具店の%s"):format(name)
        end,
        DyeVendor = function(name)
            return ("染色店の%s"):format(name)
        end,
        Fanatic = { "オパートスの信者", "マニの信者", "エヘカトルの信者" },
        Fence = function(name)
            return ("盗賊店の%s"):format(name)
        end,
        Fisher = function(name)
            return ("釣具店の%s"):format(name)
        end,
        FoodVendor = function(name)
            return ("食品店%s"):format(name)
        end,
        GeneralVendor = function(name)
            return ("雑貨屋の%s"):format(name)
        end,
        GoodsVendor = function(name)
            return ("何でも屋の%s"):format(name)
        end,
        HorseMaster = function(name)
            return ("馬屋の%s"):format(name)
        end,
        Innkeeper = function(name)
            return ("宿屋の%s"):format(name)
        end,
        MagicVendor = function(name)
            return ("魔法店の%s"):format(name)
        end,
        OfDerphy = function(name)
            return ("ダルフィ%s"):format(name)
        end,
        OfLumiest = function(name)
            return ("ルミエストの%s"):format(name)
        end,
        OfNoyel = function(name)
            return ("ノイエルの%s"):format(name)
        end,
        OfPalmia = function(name)
            return ("パルミア市街地の%s"):format(name)
        end,
        OfPortKapul = function(name)
            return ("ポート・カプールの%s"):format(name)
        end,
        OfVernis = function(name)
            return ("ヴェルニースの%s"):format(name)
        end,
        OfYowyn = function(name)
            return ("ヨウィンの%s"):format(name)
        end,
        Shade = "シェイド",
        SlaveMaster = "謎の奴隷商人",
        SouvenirVendor = function(name)
            return ("おみやげ屋の%s"):format(name)
        end,
        SpellWriter = function(name)
            return ("魔法書作家の%s"):format(name)
        end,
        StreetVendor = function(name)
            return ("屋台商人の%s"):format(name)
        end,
        StreetVendor2 = function(name)
            return ("屋台商人屋の%s"):format(name)
        end,
        Trader = function(name)
            return ("交易店の%s"):format(name)
        end,
        Trainer = function(name)
            return ("ギルドの%s"):format(name)
        end,
        WanderingVendor = function(name)
            return ("行商人の%s"):format(name)
        end,
    },
}
