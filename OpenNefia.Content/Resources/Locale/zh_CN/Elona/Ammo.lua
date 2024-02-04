Elona.Ammo = {
    NeedToEquip = "未装备箭弹。",
    IsNotCapableOfSwitching = function(item)
        return ("%s无法切换。"):format(_.name(item))
    end,

    Current = "当前装填子弹:",

    Name = {
        Normal = "普通弹",
    },
    Capacity = {
        Unlimited = "无限",
    },
}