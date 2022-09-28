Elona.Inventory.Behavior = {
    Examine = {
        WindowTitle = "調べる",
        QueryText = "どのアイテムを調べる？",

        KeyHints = {
            MultiDrop = "連続で置く",
            NoDrop = "保持指定",
        },

        NoDrop = {
            Set = function(entity)
                return ("%sを大事なものに指定した。"):format(_.name(entity))
            end,
            Unset = function(entity)
                return ("%sはもう大事なものではない。"):format(_.name(entity))
            end,
        },
    },

    Drop = {
        WindowTitle = "置く",
        QueryText = "どのアイテムを置く？",
        HowMany = function(min, max, entity)
            return ("%sをいくつ落とす？ (%s〜%s) "):format(_.name(entity), min, max)
        end,
    },

    PickUp = {
        WindowTitle = "拾う",
        QueryText = "どのアイテムを拾う？",
    },

    Eat = {
        WindowTitle = "食べる",
        QueryText = "何を食べよう？",
    },

    Equip = {
        WindowTitle = "装備する",
        QueryText = "何を装備する？",
    },

    Read = {
        WindowTitle = "読む",
        QueryText = "どれを読む？",
    },

    Drink = {
        WindowTitle = "飲む",
        QueryText = "どれを飲む？",
    },

    Zap = {
        WindowTitle = "振る",
        QueryText = "どれを振る？",
    },

    Give = {
        WindowTitle = "渡す",
        QueryText = "どれを渡す？",
    },

    Buy = {
        WindowTitle = "購入する",
        QueryText = "どれを購入する？",
        HowMany = function(min, max, entity)
            return ("%sをいくつ買う？ (%s〜%s)"):format(_.name(entity), min, max)
        end,

        PromptConfirm = function(item, cost)
            return ("%sを %s gp で買う？"):format(_.name(item), cost)
        end,
        NotEnoughMoney = {
            "あなたは財布を開いてがっかりした…",
            "もっと稼がないと買えない！",
        },
        YouBuy = function(item)
            return ("%sを買った。"):format(_.name(item))
        end,
    },

    Identify = {
        WindowTitle = "鑑定する",
        QueryText = "どのアイテムを鑑定する？",
    },

    Sell = {
        WindowTitle = "売却する",
        QueryText = "どれを売却する？",
        HowMany = function(min, max, entity)
            return ("%sをいくつ売る？ (%s〜%s)"):format(_.name(entity), min, max)
        end,

        PromptConfirm = function(item, price)
            return ("%sを %s gp で売る？"):format(_.name(item), price)
        end,
        NotEnoughMoney = function(shopkeeper)
            return ("%sは財布を開いてがっかりした…"):format(_.name(shopkeeper))
        end,
        ShopkeepersInventoryIsFull = "店の倉庫が一杯のため売れない。",
        YouSell = {
            Normal = function(item)
                return ("%sを売った。"):format(_.name(item))
            end,
            Stolen = function(item)
                return ("盗品の%sを売却した。"):format(_.name(item))
            end,
        },
    },

    Use = {
        WindowTitle = "使う",
        QueryText = "どのアイテムを使用する？",
    },

    Open = {
        WindowTitle = "開く",
        QueryText = "どれを開ける？",
    },

    Cook = {
        WindowTitle = "料理する",
        QueryText = "何を料理する？",
    },

    Mix = {
        WindowTitle = "調合",
        QueryText = "何を混ぜる？",
    },

    MixTarget = {
        WindowTitle = "混ぜる対象",
        QueryText = function(item)
            return ("何に混ぜる？(%sの効果を適用するアイテムを選択) "):format(_.name(item))
        end,
    },

    Offer = {
        WindowTitle = "捧げる",
        QueryText = "何を神に捧げる？",
    },

    Trade = {
        WindowTitle = "交換する",
        QueryText = "何を交換する？",
    },

    TradeTarget = {
        WindowTitle = "交換する",
        QueryText = "何と交換する？",
    },

    Present = {
        WindowTitle = "提示する",
        PromptConfirm = function(item, itemAmount, targetItem, targetAmount)
            return ("本当に%sの代わりに%sを提示する？"):format(
                _.name(targetItem, true, targetAmount),
                _.name(item, true, itemAmount)
            )
        end,
        QueryText = function(item, itemAmount)
            return ("%sの代わりに何を提示する？ "):format(_.name(item, true, itemAmount))
        end,
        TooLowValue = function(targetItem, targetAmount)
            return ("%sに見合う物を所持していない。"):format(_.name(targetItem, true, targetAmount))
        end,
        TooLowValueAmount = function(offerItem, offerAmount, targetItem, targetAmount)
            return ("%sは%sに見合えない。"):format(
                _.name(offerItem, true, offerAmount),
                _.name(targetItem, true, targetAmount)
            )
        end,
        CannotUnequip = function(owner, item)
            return ("%sは%sを外せない."):format(_.name(owner, true), _.name(item, true))
        end,
        YouReceive = function(offerItem, offerAmount, targetItem, targetAmount)
            return ("%sを%sと交換した。"):format(
                _.name(targetItem, true, targetAmount),
                _.name(offerItem, true, offerAmount)
            )
        end,
    },

    Throw = {
        WindowTitle = "投げる",
        QueryText = "何を投げる？",
    },

    Steal = {
        WindowTitle = "盗む",
        QueryText = "何を盗む？",
    },

    Take = {
        WindowTitle = "取る",
        QueryText = "何を取る？",
    },

    Put = {
        WindowTitle = "入れる",
        QueryText = "何を入れる？",
    },

    Target = {
        WindowTitle = "対象の",
        QueryText = "何を対象にする？",
    },
}
