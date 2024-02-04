Elona.Throw = {
    Throws = function(entity, item)
        return ("%s投掷了%s。"):format(_.name(entity), _.name(item))
    end,
    Hits = function(entity)
        return ("命中了%s！"):format(_.name(entity))
    end,
}