Elona.Item.Torch = {
    Light = function(entity, item)
        return ("%s%sを灯した。"):format(_.sore_wa(entity), _.basename(item))
    end,
    PutOut = function(entity, item)
        return ("%s%sを消した。"):format(_.sore_wa(entity), _.basename(item))
    end,
}
