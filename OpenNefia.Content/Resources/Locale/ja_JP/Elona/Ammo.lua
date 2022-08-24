Elona.Ammo = {
    NeedToEquip = "矢弾を装備していない。",
    IsNotCapableOfSwitching = function(item)
        return ("%sは切り替えに対応していない。"):format(_.name(item))
    end,

    Current = "現在の装填弾:",

    Name = {
        Normal = "通常弾",
    },
    Capacity = {
        Unlimited = "無限",
    },
}
