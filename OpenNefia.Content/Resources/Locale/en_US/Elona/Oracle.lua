Elona.Oracle = {
    Message = {
        WasCreatedAt = function(itemName, mapName, areaName, date)
            return ("%s was created at %s in %s/%s, %s. "):format(itemName, mapName, date.Day, date.Month, date.Year)
        end,
        WasHeldBy = function(itemName, mapName, areaName, ownerName, date)
            return ("%s was held by %s at %s in %s/%s, %s. "):format(
                itemName,
                ownerName,
                areaName,
                date.Day,
                date.Month,
                date.Year
            )
        end,
    },

    Effect = {
        NoArtifacts = "No artifacts have been generated yet.",
        Cursed = "You hear a sepulchral whisper but the voice is too small to distinguish a word.",
    },

    ConvertArtifact = function(oldItemName, item)
        return ("%s turns its shape into %s."):format(oldItemName, _.name(item, true))
    end,
}
