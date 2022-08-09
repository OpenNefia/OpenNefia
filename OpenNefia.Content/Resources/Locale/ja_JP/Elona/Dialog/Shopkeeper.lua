Elona.Dialog.Shopkeeper = {
    Choices = {
        Buy = "買いたい",
        Sell = "売りたい",
        Invest = "投資したい",
        Ammo = "矢弾の充填",
        Attack = "襲撃するよ",
    },

    Invest = {
        Choices = {
            Invest = "投資する",
            GoBack = "やめる",
        },
        Ask = function(speaker, goldCost)
            return ("投資をしてくれる%s%s goldかかるけどいいの%s"):format(
                _.noka(speaker, 1),
                goldCost,
                _.kana(speaker, 1)
            )
        end,
    },

    Ammo = {
        Choices = {
            Pay = "頼む",
            GoBack = "やめる",
        },
        Cost = function(speaker, goldCost)
            return ("そう%s、全ての矢弾を補充すると%s gold%s"):format(
                _.dana(speaker, 3),
                goldCost,
                _.da(speaker)
            )
        end,
        NoAmmo = function(speaker)
            return ("充填する必要はないみたい%s"):format(_.da(speaker))
        end,
    },

    Attack = {
        Choices = {
            Attack = "神に祈れ",
            GoBack = "いや、冗談です",
        },
        Dialog = function(speaker)
            return ("%s"):format(_.rob(speaker, 2))
        end,
    },

    Criminal = {
        Buy = function(speaker)
            return ("犯罪者に売る物はない%s"):format(_.yo(speaker))
        end,
        Sell = function(speaker)
            return ("犯罪者から買う物はない%s"):format(_.yo(speaker))
        end,
    },
}
