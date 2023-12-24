Elona.Return = {
    Prompt = "どの場所に帰還する？",
    LevelCounter = function(level)
        return ("%s階"):format(level)
    end,
    NoLocations = "この大陸には帰還できる場所が無い。",
    Begin = "周囲の大気がざわめきだした。",
    Cancel = function(source)
        return ("%s帰還を中止した。"):format(_.sore_wa(source))
    end,
    Prevented = "不思議な力が帰還を阻止した。",
}
