Elona.Cargo = {
    ItemName = {
        BuyingPrice = function(price)
            return ("(仕入れ値 %sg)"):format(price)
        end,
    },

    Burdened = "荷車の重量超過でかなり鈍足になっている！ ",
}
