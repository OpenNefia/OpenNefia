Elona.Chargeable = {
    ItemName = {
        Charges = function(charges)
            return ("(剩余%s次)"):format(charges)
        end,
    },

    DrawCharge = {
        Extract = function(source, item, chargesAbsorbed, totalCharges)
            return ("%s抽取了%s的魔力(共计%s)，摧毁了%s"):format(
                _.sore_wa(source),
                _.name(item, true),
                chargesAbsorbed,
                totalCharges
            )
        end,
    },

    Recharge = {
        Errors = {
            NotEnoughPower = function(source, item, chargesNeeded, totalCharges)
                return ("%s充填%s需要至少%s魔力储备(剩余%s)"):format(
                    _.sore_wa(source),
                    _.name(item, true),
                    chargesNeeded,
                    totalCharges
                )
            end,
            CannotRecharge = "无法充填该物品。",
            CannotRechargeAnymore = function(source, item)
                return ("%s无法再继续充填。"):format(_.name(item))
            end,
        },
        SpendPower = function(source, chargesNeeded, totalCharges)
            return ("%s消耗了%s魔力储备(剩余%s)"):format(
                _.sore_wa(source),
                chargesNeeded,
                totalCharges
            )
        end,
        Success = function(source, item, amount)
            return ("%s已充填(%s)"):format(_.name(item), amount)
        end,
        Failure = {
            Explodes = function(source, item)
                return ("%s爆炸了。"):format(_.name(item))
            end,
            FailToRecharge = function(source, item)
                return ("%s无法充填%s。"):format(_.sore_wa(source), _.name(item))
            end,
        },
    },
}