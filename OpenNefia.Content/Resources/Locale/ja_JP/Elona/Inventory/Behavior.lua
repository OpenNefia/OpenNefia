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
        QueryTargetText = "何を交換する？ ",
        QueryExchangingForText = "何と交換する？",
    },

    Present = {
        WindowTitle = "提示する",
        QueryText = function(item)
            return ("%sの代わりに何を提示する？ "):format(item)
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
