Elona.Chest = {
    ItemName = {
        Level = function(lv)
            return ("Level %s"):format(lv)
        end,
        Empty = "(Empty)",
        Temporal = "(Temporal)",
    },

    Open = {
        Empty = "It's empty!",
        YouOpen = function(user, item)
            return ("%s open%s %s."):format(_.name(user), _.s(user), _.name(item))
        end,
        Goods = function(item)
            return ("Several quality goods spread out from %s."):format(_.name(item))
        end,
    },
}
