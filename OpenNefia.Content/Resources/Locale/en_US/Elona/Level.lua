Elona.Level = {
    Gain = {
        Player = function(entity)
            return ("%s %s gained a level."):format(_.name(entity), _.has(entity))
        end,
        Other = function(entity)
            return ("%s %s grown up."):format(_.name(entity), _.has(entity))
        end,
    },
}
