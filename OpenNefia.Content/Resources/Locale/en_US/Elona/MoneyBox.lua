Elona.MoneyBox = {
    ItemName = {
        Increments = {
            ["500"] = "500 GP",
            ["2000"] = "2k GP",
            ["10000"] = "10K GP",
            ["50000"] = "50K GP",
            ["500000"] = "500K GP",
            ["5000000"] = "5M GP",
            ["100000000"] = "500M GP",
        },
        Amount = function(name, amount)
            return ("%s(%s)"):format(name, amount)
        end,
    },
    NotEnoughGold = "You count your coins and sigh...",
    Full = "The money box is full.",
}
