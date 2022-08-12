Elona.Throw = {
    Throws = function(entity, item)
        return ("%s throw%s %s."):format(_.name(entity), _.s(entity), _.name(item))
    end,
    Hits = function(entity)
        return ("It hits %s!"):format(_.name(entity))
    end,
}
