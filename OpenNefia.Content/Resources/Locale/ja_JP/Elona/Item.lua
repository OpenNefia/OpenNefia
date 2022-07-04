Elona.Item = {
    NameModifiers = {
        Great = function(name)
            return ("『%s』"):format(name)
        end,
        God = function(name)
            return ("《%s》"):format(name)
        end,
        Article = function(name)
            return ("%s"):format(name)
        end,
    },
}
