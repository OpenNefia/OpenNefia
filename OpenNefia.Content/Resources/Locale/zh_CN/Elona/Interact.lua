Elona.Interact = {
    Query = {
        Direction = "操作对象的方向是？",
        Action = function(target)
            return ("%s要做什么？"):format(_.name(target))
        end,
    },
    NoInteractActions = function(target)
        return ("无法操作%s。"):format(_.name(target))
    end,

    Actions = {
        Appearance = "换衣服",
        Attack = "攻击",
        BringOut = "带出来",
        ChangeTone = "改变口吻",
        Equipment = "装备",
        Give = "给予",
        Info = "信息",
        Inventory = "背包",
        Items = "物品",
        Name = "起名字",
        Release = "解开绳子",
        Talk = "交谈",
        TeachWords = "教授单词",
    },
}