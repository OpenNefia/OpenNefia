Elona.Chest = {
    ItemName = {
        Level = function(lv)
            return ("Lv%s"):format(lv)
        end,
        Empty = "(空)",
        Temporal = "(移动时消失)",
    },

    Open = {
        Empty = "内部是空的。",
        YouOpen = function(user, item)
            return ("%s打开了%s。"):format(_.name(user), _.name(item))
        end,
        Goods = function(item)
            return ("从%s中溢出了许多高级物品，散落在脚下。"):format(_.name(item))
        end,
    },
}