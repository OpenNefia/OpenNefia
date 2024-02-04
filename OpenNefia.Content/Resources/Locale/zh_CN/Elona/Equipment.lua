Elona.Equipment = {
    ArmorClass = {
        Light = "(轻型装备)",
        Medium = "(中型装备)",
        Heavy = "(重型装备)",
    },

    YouChangeYourEquipment = "装备已更换。",

    Layer = {
        Window = {
            Title = "装备",
        },

        Topic = {
            CategoryName = "部位/装备名称",
            Weight = "重量",
        },

        Stats = {
            EquipWeight = "装备重量",
            HitBonus = "命中修正",
            DamageBonus = "伤害修正",
        },

        MainHand = "主手",
    },

    Suitability = {
        TwoHand = {
            FitsWell = function(actor, target, item)
                return ("目前装备的%s很适合双手使用。"):format(_.name(item))
            end,
            TooLight = function(actor, target, item)
                return ("目前装备的%s对于双手持有来说稍微有点轻。"):format(_.name(item))
            end,
        },
        DualWield = {
            TooHeavy = {
                MainHand = function(actor, target, item)
                    return ("目前装备的%s对于主手来说太重了。"):format(_.name(item))
                end,
                SubHand = function(actor, target, item)
                    return ("目前装备的%s对于副手来说太重了。"):format(_.name(item))
                end,
            },
        },
        Riding = {
            TooHeavy = function(actor, target, item)
                return ("目前装备的%s对于骑乘来说太重了。"):format(_.name(item))
            end,
        },
    },
}