Elona.MoneyBox = {
    ItemName = {
        Increments = {
            ["500"] = "5百金貨",
            ["2000"] = "2千金貨",
            ["10000"] = "1万金貨",
            ["50000"] = "5万金貨",
            ["500000"] = "50万金貨",
            ["5000000"] = "500万金貨",
            ["100000000"] = "1億金貨",
        },
        Amount = function(name, amount)
            return ("%s%s"):format(name, amount)
        end,
    },
    NotEnoughGold = "金貨が足りない…",
    Full = "貯金箱は一杯だ。",
}
