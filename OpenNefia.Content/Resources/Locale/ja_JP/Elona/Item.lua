Elona.Item = {
    NameModifiers = {
        Great = function(name)
            return ("ã%sã"):format(name)
        end,
        God = function(name)
            return ("ã%sã"):format(name)
        end,
        Article = function(name)
            return ("%s"):format(name)
        end,
    },
}
