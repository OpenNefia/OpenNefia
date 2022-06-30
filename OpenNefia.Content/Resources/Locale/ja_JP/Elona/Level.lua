Elona.Level = {
    Gain = {
        Player = function(entity, level)
            return ("%sはレベル%sになった！"):format(_.name(entity), level)
        end,
        Other = function(entity)
            return ("%sは成長した。"):format(_.name(entity))
        end,
    },
}
