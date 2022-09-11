Elona.Scroll = {
    Read = {
        DimmedOrConfused = function(reader)
            return ("%sはふらふらした。"):format(_.name(reader))
        end,
        Execute = function(reader, scroll)
            return ("%s%sを読んだ。"):format(_.sore_wa(reader), _.name(scroll))
        end,
    },
}
