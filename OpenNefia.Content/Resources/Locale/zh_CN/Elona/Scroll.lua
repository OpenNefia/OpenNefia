Elona.Scroll = {
    Read = {
        DimmedOrConfused = function(reader)
            return ("%s感到晕晕乎乎的。"):format(_.name(reader))
        end,
        Execute = function(reader, scroll)
            return ("%s读了%s。"):format(_.sore_wa(reader), _.name(scroll, nil, 1))
        end,
    },
}