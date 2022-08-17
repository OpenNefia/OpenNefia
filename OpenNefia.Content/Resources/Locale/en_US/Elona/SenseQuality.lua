Elona.SenseQuality = {
    ItemName = function(quality, materialName)
        return (" (%s)[%s]"):format(_.capitalize(quality), _.capitalize(materialName))
    end,

    Identify = {
        AlmostIdentified = function(unidentified, quality)
            return ("You sense the quality of %s is %s."):format(unidentified, quality)
        end,
        FullyIdentified = function(unidentified, identified)
            return ("You appraise %s as %s."):format(unidentified, identified)
        end,
    },

    CurseStates = {
        Cursed = "(Scary)",
        Doomed = "(Dreadful)",
    },
}
