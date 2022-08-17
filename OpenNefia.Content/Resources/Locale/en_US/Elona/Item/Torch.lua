Elona.Item.Torch = {
    ItemName = {
        Lit = "(Lit)",
    },
    Light = function(entity, item)
        return ("%s light%s up the %s."):format(_.name(entity), _.s(entity), _.basename(item))
    end,
    PutOut = function(entity, item)
        return ("%s put%s out the fire."):format(_.name(entity), _.s(entity))
    end,
}
