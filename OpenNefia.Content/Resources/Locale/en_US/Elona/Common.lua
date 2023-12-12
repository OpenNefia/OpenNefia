Elona.Common = {
    Quotes = function(str)
        return ("\"%s\""):format(str)
    end,
    ItIsImpossible = "It's impossible.",
    NothingHappens = "Nothing happens...",
    SomethingIsPut = "Something is put on the ground.",
    TooExhausted = "You are too exhausted!",
    PutInBackpack = function(entity)
        return ("You put %s in your backpack."):format(_.name(entity))
    end,
    CannotDoInGlobal = "You can't do that while you're in a global area.",
    NameWithDirectArticle = function(entity)
        return _.name(entity, true)
    end,
    QualifiedName = function(basename, itemTypeName)
        return ("%s of %s"):format(itemTypeName, basename)
    end,
}
