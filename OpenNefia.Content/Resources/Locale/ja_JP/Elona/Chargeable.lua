Elona.Chargeable = {
    ItemName = {
        Charges = function(charges)
            return ("(残り%s回)"):format(charges)
        end,
    },

    DrawCharge = {
        Extract = function(source, item, chargesAbsorbed, totalCharges)
            return ("%s%sを破壊して%sの魔力を抽出した(計%s)"):format(
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
                return ("%s%sを充填するには最低でも魔力の貯蓄が%s必要だ。(残り%s)"):format(
                    _.sore_wa(source),
                    _.name(item, true),
                    chargesNeeded,
                    totalCharges
                )
            end,
            CannotRecharge = "それは充填ができないようだ。",
            CannotRechargeAnymore = function(source, item)
                return ("%sはこれ以上充填できないようだ。"):format(_.name(item))
            end,
        },
        SpendPower = function(source, chargesNeeded, totalCharges)
            return ("%s魔力の貯蓄を%s消費した(残り%s)"):format(
                _.sore_wa(source),
                chargesNeeded,
                totalCharges
            )
        end,
        Success = function(source, item, amount)
            return ("%sは充填された(%s)"):format(_.name(item), amount)
        end,
        Failure = {
            Explodes = function(source, item)
                return ("%sは破裂した。"):format(_.name(item))
            end,
            FailToRecharge = function(source, item)
                return ("%s%sへの充填は失敗した。"):format(_.sore_wa(source), _.name(item))
            end,
        },
    },
}
