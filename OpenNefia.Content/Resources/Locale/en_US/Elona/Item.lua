Elona.Item = {
    ItemName = {
        LostProperty = "(Lost property)",
        UseInterval = function(hours)
            return ("(Next: %sh.)"):format(hours)
        end,
    },
    NameModifiers = {
        Great = function(name)
            return ("<%s>"):format(name)
        end,
        God = function(name)
            return ("{%s}"):format(name)
        end,
        Article = function(name)
            return ("The %s"):format(name)
        end,
    },
}
