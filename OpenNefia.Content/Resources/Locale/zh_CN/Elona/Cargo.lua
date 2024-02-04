Elona.Cargo = {
    ItemName = {
        BuyingPrice = function(price)
            return ("(进货价 %sg)"):format(price)
        end,
    },

    Burdened = "因运货重量超过，变得相当迟缓！ ",
}