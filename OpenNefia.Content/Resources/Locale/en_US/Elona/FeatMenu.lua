Elona.FeatMenu = {
    Title = "Feats and Traits",
    GainedHeader = "[Feats and traits]",
    AvailableHeader = "[Available feats]",
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
