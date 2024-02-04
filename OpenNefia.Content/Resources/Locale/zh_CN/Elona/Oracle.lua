Elona.Oracle = {
    Message = {
        WasCreatedAt = function(itemName, mapName, areaName, date)
            return ("%s在%s年%s月在%s被创造出来。"):format(itemName, date.Year, date.Month, mapName)
        end,
        WasHeldBy = function(itemName, mapName, areaName, ownerName, date)
            return ("%s在%s年%s月被%s的%s拥有。"):format(
                itemName,
                date.Year,
                date.Month,
                areaName,
                ownerName
            )
        end,
    },

    Effect = {
        NoArtifacts = "尚未生成特殊物品。",
        Cursed = "有什么东西在你耳边低语，但你无法听清。",
    },

    ConvertArtifact = function(oldItemName, item)
        return ("%s变成了%s。"):format(oldItemName, _.name(item, true))
    end,
}