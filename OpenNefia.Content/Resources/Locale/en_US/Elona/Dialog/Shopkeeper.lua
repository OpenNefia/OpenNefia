Elona.Dialog.Shopkeeper = {
    Choices = {
        Ammo = "I need ammos for my weapon.",
        Attack = "Prepare to die!",
        Buy = "I want to buy something.",
        Invest = "Need someone to invest in your shop?",
        Sell = "I want to sell something.",
    },

    Invest = {
        Choices = {
            Invest = "Invest",
            GoBack = "Reject",
        },
        Ask = function(speaker, goldCost)
            return ("Oh, do you want to invest in my shop? It will cost you %s golds. I hope you got the money."):format(
                goldCost
            )
        end,
    },

    Ammo = {
        Choices = {
            Pay = "Alright.",
            GoBack = "Another time.",
        },
        Cost = function(goldCost)
            return (
                "Sure, let me check what type of ammos you need....Okay, reloading all of your ammos will cost %s gold pieces."
            ):format(goldCost)
        end,
        NoAmmo = "Reload what? You don't have any ammo in your inventory.",
    },

    Attack = {
        Choices = {
            Attack = "Pray to your God.",
            GoBack = "W-Wait! I was just kidding.",
        },
        Dialog = "Oh crap. Another bandit trying to spoil my business! Form up, mercenaries.",
    },

    Criminal = {
        Buy = "I don't have business with criminals.",
        Sell = "I don't have business with criminals.",
    },
}
