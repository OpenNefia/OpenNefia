Elona.FeatMenu = {
    Window = {
        Title = "Feats and Traits",
    },
    Header = {
        Available = "[Available feats]",
        Gained = "[Feats and traits]",
    },
    FeatMax = "MAX",
    Topic = {
        Name = "Name",
        Detail = "Detail",
    },
    FeatCount = function(featsRemaining)
        return ("You can acquire %s feats"):format(featsRemaining)
    end,
    FeatType = {
        Feat = "Feat",
        Race = "Race",
        Mutation = "Mutation",
        EtherDisease = "Disease",
    },
}
