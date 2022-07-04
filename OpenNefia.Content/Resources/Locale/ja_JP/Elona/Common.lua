Elona.Common = {
    Quotes = function(str)
        return ("「%s」"):format(str)
    end,
    ItIsImpossible = "それは無理だ。",
    NothingHappens = "何もおきない… ",
    SomethingIsPut = "何かが足元に転がってきた。",
    PutInBackpack = function(entity)
        return ("%sをバックパックに入れた。"):format(_.name(entity))
    end,
    NameWithDirectArticle = function(entity)
        return _.name(entity)
    end,
}
