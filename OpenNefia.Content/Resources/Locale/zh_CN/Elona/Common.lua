Elona.Common = {
    Quotes = function(str)
        return ("「%s」"):format(str)
    end,
    ItIsImpossible = "这是不可能的。",
    NothingHappens = "什么都没有发生...",
    SomethingIsPut = "有什么东西滚到了脚下。",
    TooExhausted = function(entity)
        entity = entity or _.player()
        return ("%s因过度疲劳而失败！"):format(_.sore_wa(entity))
    end,
    PutInBackpack = function(item)
        return ("将%s放入背包。"):format(_.name(item))
    end,
    CannotDoInGlobal = "在世界地图上无法进行此行动。",
    DoesNotWorkHere = "这个地方没有效果。",
    NameWithDirectArticle = function(entity)
        return _.name(entity, true)
    end,
    QualifiedName = function(basename, itemTypeName)
        return basename
    end,
}