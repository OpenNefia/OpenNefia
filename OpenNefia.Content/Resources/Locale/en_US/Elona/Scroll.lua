Elona.Scroll = {
    Read = {
        DimmedOrConfused = function(reader)
            return ("%s stagger%s."):format(_.name(reader), _.s(reader))
        end,
        Execute = function(reader, scroll)
            return ("%s read%s %s."):format(_.name(reader), _.s(reader), _.name(scroll, nil, 1))
        end,
    },
}
