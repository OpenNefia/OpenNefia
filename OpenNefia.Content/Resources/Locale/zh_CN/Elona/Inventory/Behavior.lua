Elona.Inventory.Behavior = {
    Examine = {
        WindowTitle = "查看",
        QueryText = "要查看哪个物品？",

        KeyHints = {
            MultiDrop = "连续放置",
            NoDrop = "指定保留",
        },

        NoDrop = {
            Set = function(entity)
                return string.format("将%s设为重要物品。", _.name(entity))
            end,
            Unset = function(entity)
                return string.format("%s不再是重要物品。", _.name(entity))
            end,
        },
    },

    Drop = {
        WindowTitle = "放置",
        QueryText = "要放置哪个物品？",
        HowMany = function(min, max, entity)
            return string.format("要放置%s个%s？（%s~%s）", _.name(entity), min, max)
        end,
    },

    PickUp = {
        WindowTitle = "拾取",
        QueryText = "要拾取哪个物品？",
    },

    Eat = {
        WindowTitle = "吃",
        QueryText = "要吃什么？",
    },

    Equip = {
        WindowTitle = "装备",
        QueryText = "要装备哪个物品？",
    },

    Read = {
        WindowTitle = "阅读",
        QueryText = "要阅读哪个物品？",
    },

    Drink = {
        WindowTitle = "喝",
        QueryText = "要喝哪个物品？",
    },

    Zap = {
        WindowTitle = "挥舞",
        QueryText = "要挥舞哪个物品？",
    },

    Give = {
        WindowTitle = "给予",
        QueryText = "要给予哪个物品？",
    },

    Buy = {
        WindowTitle = "购买",
        QueryText = "要购买哪个物品？",
        HowMany = function(min, max, entity)
            return string.format("要购买%s个%s？（%s~%s）", _.name(entity), min, max)
        end,

        PromptConfirm = function(item, cost)
            return string.format("以%s gp购买%s？", cost, _.name(item))
        end,
        NotEnoughMoney = {
            "你打开钱包感到失望...",
            "没有足够的金钱购买！",
        },
        YouBuy = function(item)
            return string.format("购买了%s。", _.name(item))
        end,
    },

    Identify = {
        WindowTitle = "鉴定",
        QueryText = "要鉴定哪个物品？",
    },

    Sell = {
        WindowTitle = "出售",
        QueryText = "要出售哪个物品？",
        HowMany = function(min, max, entity)
            return string.format("要出售%s个%s？（%s~%s）", _.name(entity), min, max)
        end,

        PromptConfirm = function(item, price)
            return string.format("以%s gp出售%s？", price, _.name(item))
        end,
        NotEnoughMoney = function(shopkeeper)
            return string.format("%s打开钱包感到失望...", _.name(shopkeeper))
        end,
        ShopkeepersInventoryIsFull = "商店的库存已满，无法出售。",
        YouSell = {
            Normal = function(item)
                return string.format("出售了%s。", _.name(item))
            end,
            Stolen = function(item)
                return string.format("出售了被偷窃的%s。", _.name(item))
            end,
        },
    },

    Use = {
        WindowTitle = "使用",
        QueryText = "要使用哪个物品？",
    },

    Open = {
        WindowTitle = "打开",
        QueryText = "要打开哪个物品？",
    },

    Cook = {
        WindowTitle = "烹饪",
        QueryText = "要烹饪什么？",
    },

    Mix = {
        WindowTitle = "混合",
        QueryText = "要混合什么？",
    },

    MixTarget = {
        WindowTitle = "混合目标",
        QueryText = function(item)
            return string.format("要与之混合？（选择应用%s效果的物品）", _.name(item))
        end,
    },

    Offer = {
        WindowTitle = "献祭",
        QueryText = "要献祭哪个物品？",
    },

    Trade = {
        WindowTitle = "交换",
        QueryText = "要交换哪个物品？",
    },

    TradeTarget = {
        WindowTitle = "交换",
        QueryText = "要与之交换？",
    },

    Present = {
        WindowTitle = "赠送",
        PromptConfirm = function(item, itemAmount, targetItem, targetAmount)
            return string.format("确定以%s换取%s？", _.name(targetItem, true, targetAmount), _.name(item, true, itemAmount))
        end,
        QueryText = function(item, itemAmount)
            return string.format("以%s作为替代，赠送什么？", _.name(item, true, itemAmount))
        end,
        TooLowValue = function(targetItem, targetAmount)
            return string.format("没有拥有足够与%s相等的物品。", _.name(targetItem, true, targetAmount))
        end,
        TooLowValueAmount = function(offerItem, offerAmount, targetItem, targetAmount)
            return string.format("%s无法达到%s的效果。", _.name(offerItem, true, offerAmount), _.name(targetItem, true, targetAmount))
        end,
        CannotUnequip = function(owner, item)
            return string.format("%s无法脱下%s。", _.name(owner, true), _.name(item, true))
        end,
        YouReceive = function(offerItem, offerAmount, targetItem, targetAmount)
            return string.format("交换了%s。", _.name(targetItem, true, targetAmount), _.name(offerItem, true, offerAmount))
        end,
    },

    Throw = {
        WindowTitle = "扔",
        QueryText = "要扔哪个物品？",
    },

    Steal = {
        WindowTitle = "偷窃",
        QueryText = "要偷窃哪个物品？",
    },

    Take = {
        WindowTitle = "拿",
        QueryText = "要拿取哪个物品？",
    },

    Put = {
        WindowTitle = "放入",
        QueryText = "要放入哪个物品？",
    },

    Target = {
        WindowTitle = "选择目标",
        QueryText = "要选择哪个物品作为目标？",
    },
}