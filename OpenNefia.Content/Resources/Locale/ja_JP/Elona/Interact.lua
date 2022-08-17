Elona.Interact = {
    Query = {
        Direction = "操作する対象の方向は？",
        Action = function(target)
            return ("%sに何をする？"):format(_.name(target))
        end,
    },
    NoInteractActions = function(target)
        return ("%sを操作できない。"):format(_.name(target))
    end,

    Actions = {
        Appearance = "着替えさせる",
        Attack = "攻撃する",
        BringOut = "連れ出す",
        ChangeTone = "口調を変える",
        Equipment = "装備",
        Give = "何かを渡す",
        Info = "情報",
        Inventory = "所持品",
        Items = "アイテム",
        Name = "名前をつける",
        Release = "縄を解く",
        Talk = "話しかける",
        TeachWords = "言葉を教える",
    },
}
