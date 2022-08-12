Elona.Item = {
    ItemName = {
        EternalForce = "eternal force",
        LostProperty = "(Lost property)",
        UseInterval = function(hours)
            return ("(Next: %sh.)"):format(hours)
        end,
        FromEntity = function(name)
            return ("of %s"):format(name)
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
