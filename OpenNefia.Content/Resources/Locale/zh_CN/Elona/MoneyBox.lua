Elona.MoneyBox = {
    ItemName = {
        Increments = {
            ["500"] = "5百金币",
            ["2000"] = "2千金币",
            ["10000"] = "1万金币",
            ["50000"] = "5万金币",
            ["500000"] = "50万金币",
            ["5000000"] = "500万金币",
            ["100000000"] = "1亿金币",
        },
        Amount = function(name, amount)
            return ("%s%s"):format(name, amount)
        end,
    },
    NotEnoughGold = "金币不足...",
    Full = "储蓄箱已满。",
}