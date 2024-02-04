Elona.Dialog.Shopkeeper = {
    Choices = {
        Buy = "购买",
        Sell = "出售",
        Invest = "投资",
        Ammo = "弹药充填",
        Attack = "发起攻击",
    },

    Invest = {
        Choices = {
            Invest = "投资",
            GoBack = "返回",
        },
        Ask = function(speaker, goldCost)
            return ("我需要投资%s%s金币，可以吗%s"):format(
                _.noka(speaker, 1),
                goldCost,
                _.kana(speaker, 1)
            )
        end,
    },

    Ammo = {
        Choices = {
            Pay = "支付",
            GoBack = "返回",
        },
        Cost = function(speaker, goldCost)
            return ("你好%s，如果要补充所有弹药需要%s金币%s"):format(
                _.dana(speaker, 3),
                goldCost,
                _.da(speaker)
            )
        end,
        NoAmmo = function(speaker)
            return ("看来不需要充填%s"):format(_.da(speaker))
        end,
    },

    Attack = {
        Choices = {
            Attack = "向神祈祷",
            GoBack = "不，开个玩笑",
        },
        Dialog = function(speaker)
            return ("%s"):format(_.rob(speaker, 2))
        end,
    },

    Criminal = {
        Buy = function(speaker)
            return ("我没有东西可以卖给罪犯%s"):format(_.yo(speaker))
        end,
        Sell = function(speaker)
            return ("我没有东西可以从罪犯那里购买%s"):format(_.yo(speaker))
        end,
    },
}