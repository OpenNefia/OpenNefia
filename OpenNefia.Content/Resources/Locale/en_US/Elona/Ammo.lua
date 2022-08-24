Elona.Ammo = {
    NeedToEquip = "You need to equip an ammo.",
    IsNotCapableOfSwitching = function(item)
        return ("%s isn't capable of changing ammos."):format(_.name(item))
    end,

    Current = "Current Ammo Type:",

    Name = {
        Normal = "Normal",
    },
    Capacity = {
        Unlimited = "Unlimited",
    },
}
