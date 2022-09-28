Elona.Oracle = {
    WasCreatedAt = function(item, map, day, month, year)
        return ("%s was created at %s in %s/%s, %s. "):format(_.basename(item), _.name(map), day, month, year)
    end,
    WasHeldBy = function(item, owner, map, day, month, year)
        return ("%s was held by %s at %s in %s/%s, %s. "):format(
            _.basename(item),
            _.basename(owner),
            _.name(map),
            day,
            month,
            year
        )
    end,

    Effect = {
        NoArtifacts = "No artifacts have been generated yet.",
        Cursed = "You hear a sepulchral whisper but the voice is too small to distinguish a word.",
    },

    ConvertArtifact = function(oldItemName, item)
        return ("%s turns its shape into %s."):format(oldItemName, _.name(item, true))
    end,
}
