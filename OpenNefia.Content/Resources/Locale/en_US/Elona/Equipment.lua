Elona.Equipment = {
    ArmorClass = {
        Light = "(Light)",
        Medium = "(Medium)",
        Heavy = "(Heavy)",
    },

    YouChangeYourEquipment = "You change your equipment.",

    Layer = {
        Window = {
            Title = "Equipment",
        },

        Topic = {
            CategoryName = "Category/Name",
            Weight = "Weight",
        },

        Stats = {
            EquipWeight = "Equip weight",
            HitBonus = "Hit Bonus",
            DamageBonus = "Damage Bonus",
        },

        MainHand = "Hand*",
    },

    Suitability = {
        TwoHand = {
            FitsWell = function(actor, target, item)
                return ("%s fit%s well for two-hand fighting style."):format(_.name(item), _.s(item))
            end,
            TooLight = function(actor, target, item)
                return ("%s %s too light for two-hand fighting style."):format(_.name(item), _.is(item))
            end,
        },
        DualWield = {
            TooHeavy = {
                MainHand = function(actor, target, item)
                    return ("%s %s too heavy for two-wield fighting style."):format(_.name(item), _.is(item))
                end,
                SubHand = _.ref "Elona.EquipSlots.Suitability.TwoHand.TooHeavy.MainHand",
            },
        },
        Riding = {
            TooHeavy = function(actor, target, item)
                return ("%s %s too heavy to use when riding."):format(_.name(item), _.is(item))
            end,
        },
    },
}
