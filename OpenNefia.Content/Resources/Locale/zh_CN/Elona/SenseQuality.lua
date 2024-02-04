Elona.SenseQuality = {
    ItemName = function(quality, materialName)
        return (" (%s)[%s制]"):format(quality, materialName)
    end,

    Identify = {
        AlmostIdentified = function(unidentified, quality)
            return ("在背包中的%s似乎是%s。"):format(unidentified, quality)
        end,
        FullyIdentified = function(unidentified, identified)
            return ("在背包中的%s被确认为%s。"):format(unidentified, identified)
        end,
    },

    CurseStates = {
        Cursed = "(被诅咒的)",
        Doomed = "(注定的)",
    },
}