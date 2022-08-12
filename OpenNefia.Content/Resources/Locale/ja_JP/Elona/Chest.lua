Elona.Chest = {
    ItemName = {
        Level = function(lv)
            return ("Lv%s"):format(lv)
        end,
        Empty = "(空っぽ)",
        Temporal = "(移動時消滅)",
    },

    Open = {
        Empty = "中身は空っぽだ。",
        YouOpen = function(user, item)
            return ("%sは%sを開けた。"):format(_.name(user), _.name(item))
        end,
        Goods = function(item)
            return ("%sから溢れ出た高級品が、足元に散らばった。"):format(_.name(item))
        end,
    },
}
