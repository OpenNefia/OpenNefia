Elona.Identify = {
    Result = {
        NeedMorePower = function(source, item)
            return ("%s新しい知識は得られなかった。より上位の鑑定で調べる必要がある。"):format(
                _.sore_wa(source)
            )
        end,
        Partially = function(source, item, prevItemName)
            return ("それは%sだと判明したが、完全には鑑定できなかった。"):format(_.name(item))
        end,
        Fully = function(source, item, prevItemName)
            return ("それは%sだと完全に判明した。"):format(_.name(item))
        end,
    },
}
