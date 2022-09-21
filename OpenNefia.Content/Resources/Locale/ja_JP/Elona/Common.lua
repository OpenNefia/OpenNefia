Elona.Common = {
    Quotes = function(str)
        return ("「%s」"):format(str)
    end,
    ItIsImpossible = "それは無理だ。",
    NothingHappens = "何もおきない… ",
    SomethingIsPut = "何かが足元に転がってきた。",
    TooExhausted = "疲労し過ぎて失敗した！",
    PutInBackpack = function(entity)
        return ("%sをバックパックに入れた。"):format(_.name(entity))
    end,
    CannotDoInGlobal = "その行為は、ワールドマップにいる間はできない。",
    NameWithDirectArticle = function(entity)
        return _.name(entity)
    end,
    QualifiedName = function(basename, itemTypeName)
        return basename
    end,
}
