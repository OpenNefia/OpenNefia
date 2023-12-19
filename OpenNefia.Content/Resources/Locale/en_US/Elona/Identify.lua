Elona.Identify = {
    Result = {
        NeedMorePower = function(source, item)
            return ("%s need%s higher identification to gain new knowledge."):format(_.name(source), _.s(source))
        end,
        Partially = function(source, item, prevItemName)
            return ("The item is half-identified as %s."):format(_.name(item))
        end,
        Fully = function(source, item, prevItemName)
            return ("The item is fully identified as %s."):format(_.name(item))
        end,
    },
}
