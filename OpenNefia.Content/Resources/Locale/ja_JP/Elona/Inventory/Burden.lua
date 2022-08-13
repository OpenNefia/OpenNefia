Elona.Inventory.Burden = {
    CarryTooMuch = "潰れていて動けない！ ",
    BackpackSquashing = function(entity)
        return ("%sは荷物に圧迫されもがいた。"):format(_.name(entity))
    end,
    Indicator = {
        None = "",
        Light = "重荷",
        Moderate = "圧迫",
        Heavy = "超過",
        Max = "潰れ中",
    },
}
