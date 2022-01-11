Elona.Inventory.Behavior = {
    Examine = {
        WindowTitle = "Examine",
        QueryText = "Examine what?",

        KeyHints = {
            MultiDrop = "Multi Drop",
            NoDrop = "Tag No-Drop",
        },

        NoDrop = {
            Set = function(entity)
                return ("You set %s as no-drop."):format(_.name(entity))
            end,
            Unset = function(entity)
                return ("%s is no longer set as no-drop."):format(_.name(entity))
            end,
        },
    },

    Drop = {
        WindowTitle = "Drop",
        QueryText = "Drop what?",
    },

    PickUp = {
        WindowTitle = "Pick Up",
        QueryText = "Which item do you want to pick up?",
    },

    Eat = {
        WindowTitle = "Eat",
        QueryText = "Eat what?",
    },

    Equip = {
        WindowTitle = "Wear",
        QueryText = "Equip what?",
    },

    Read = {
        WindowTitle = "Read",
        QueryText = "Read what?",
    },

    Drink = {
        WindowTitle = "Drink",
        QueryText = "Drink what?",
    },

    Zap = {
        WindowTitle = "Zap",
        QueryText = "Zap what?",
    },

    Give = {
        WindowTitle = "Give",
        QueryText = "Which item do you want to give?",
    },

    Buy = {
        WindowTitle = "Buy",
        QueryText = "What do you want to buy?",
    },

    Identify = {
        WindowTitle = "Identify",
        QueryText = "Which item do you want to identify?",
    },

    Sell = {
        WindowTitle = "Sell",
        QueryText = "What do you want to sell?",
    },

    Use = {
        WindowTitle = "Use",
        QueryText = "Use what?",
    },

    Open = {
        WindowTitle = "Open",
        QueryText = "Open what?",
    },

    Cook = {
        WindowTitle = "Cook",
        QueryText = "Cook what?",
    },

    Mix = {
        WindowTitle = "Mix",
        QueryText = "Blend what?",
    },

    MixTarget = {
        WindowTitle = "Mix Target",
        QueryText = function(item)
            return ("Which item do you want to apply the effect of %s?"):format(item)
        end,
    },

    Offer = {
        WindowTitle = "Offer",
        QueryText = "What do you want to offer to your God?",
    },

    Trade = {
        WindowTitle = "Trade",
        QueryText = "Which item do you want to trade?",
    },

    TradeTarget = {
        WindowTitle = "Trade",
        QueryText = "Trade what?",
    },

    Present = {
        WindowTitle = "Present",
        QueryText = function(item)
            return ("What do you offer for %s?"):format(item)
        end,
    },

    Throw = {
        WindowTitle = "Throw",
        QueryText = "Throw what?",
    },

    Steal = {
        WindowTitle = "Pickpocket",
        QueryText = "Steal what?",
    },

    Take = {
        WindowTitle = "Take",
        QueryText = "Take what?",
    },

    Put = {
        WindowTitle = "Put",
        QueryText = "Put what?",
    },

    Target = {
        WindowTitle = "Target",
        QueryText = "Target what?",
    },
}
