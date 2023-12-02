Elona.Role = {
    Names = {
        Alien = {
            AlienKid = "エイリアンの子供",
            Child = "の子供",
            ChildOf = function(name)
                return ("%sの子供"):format(name)
            end,
        },
        Fanatic = { "オパートスの信者", "マニの信者", "エヘカトルの信者" },
        HorseMaster = function(name)
            return ("馬屋の%s"):format(name)
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
        SpellWriter = function(name)
            return ("魔法書作家の%s"):format(name)
        end,
        Trainer = function(name)
            return ("ギルドの%s"):format(name)
        end,
    },
}
