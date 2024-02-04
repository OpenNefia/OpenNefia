Elona.Identify = {
    Result = {
        NeedMorePower = function(source, item)
            return ("%s没有获得新的知识。需要进行更高级的鉴定。"):format(
                _.sore_wa(source)
            )
        end,
        Partially = function(source, item, prevItemName)
            return ("虽然确定了它是%s，但无法完全鉴定。"):format(_.name(item))
        end,
        Fully = function(source, item, prevItemName)
            return ("完全确定了它是%s。"):format(_.name(item))
        end,
    },
}