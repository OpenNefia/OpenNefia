Elona.Throw = {
    Throws = function(entity, item)
        return ("%sは%sを投げた。"):format(_.name(entity), _.name(item))
    end,
    Hits = function(entity)
        return ("%sに見事に命中した！"):format(_.name(entity))
    end,
}
