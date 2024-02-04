Elona.Item.Torch = {
    ItemName = {
        Lit = "(已点亮)",
    },
    Light = function(entity, item)
        return ("%s点亮了%s。"):format(_.sore_wa(entity), _.basename(item))
    end,
    PutOut = function(entity, item)
        return ("%s熄灭了%s。"):format(_.sore_wa(entity), _.basename(item))
    end,
}