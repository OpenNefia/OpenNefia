Elona.SenseQuality = {
    ItemName = function(quality, materialName)
        return (" (%s)[%s製]"):format(quality, materialName)
    end,

    Identify = {
        AlmostIdentified = function(unidentified, quality)
            return ("バックパックの中の%sは%sだという感じがする。"):format(unidentified, quality)
        end,
        FullyIdentified = function(unidentified, identified)
            return ("バックパックの中の%sは%sだと判明した。"):format(unidentified, identified)
        end,
    },

    CurseStates = {
        Cursed = "(恐ろしい)",
        Doomed = "(禍々しい)",
    },
}
