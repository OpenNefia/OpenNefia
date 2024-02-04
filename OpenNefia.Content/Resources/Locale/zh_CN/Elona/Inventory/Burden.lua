Elona.Inventory.Burden = {
    CarryTooMuch = "太过负重，无法移动！",
    BackpackSquashing = function(entity)
        return ("背包被负重压得挣扎不已。"):format(_.name(entity))
    end,
    Indicator = {
        None = "",
        Light = "负重轻",
        Moderate = "负重适中",
        Heavy = "负重沉重",
        Max = "快要被压垮",
    },
}