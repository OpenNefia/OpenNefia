Elona.SenseQuality = {
    ItemName = function(quality, materialName)
        return (" (%s)[%s]"):format(_.capitalize(quality), _.capitalize(materialName))
    end,

    CurseStates = {
        Cursed = "(Scary)",
        Doomed = "(Dreadful)",
    },
}
