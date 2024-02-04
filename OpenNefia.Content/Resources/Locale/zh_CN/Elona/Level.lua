Elona.Level = {
    Gain = {
        Player = function(entity, level)
            return ("%s达到了%s级！"):format(_.name(entity), level)
        end,
        Other = function(entity)
            return ("%s成长了。"):format(_.name(entity))
        end,
    },
}