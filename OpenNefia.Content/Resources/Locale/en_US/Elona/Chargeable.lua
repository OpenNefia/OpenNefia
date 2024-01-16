Elona.Chargeable = {
    ItemName = {
        Charges = function(charges)
            return ("(Charges: %s)"):format(charges)
        end,
    },

    DrawCharge = {
        Extract = function(source, item, chargesAbsorbed, totalCharges)
            return ("%s destroy%s %s and extract%s %s recharge powers. (Total:%s)"):format(
                _.name(source),
                _.s(source),
                _.name(item, true),
                _.s(source),
                chargesAbsorbed,
                totalCharges
            )
        end,
    },

    Recharge = {
        Errors = {
            NotEnoughPower = function(source, item, chargesNeeded, totalCharges)
                return ("%s need%s at least %s recharge powers to recharge %s. (Total: %s)"):format(
                    _.name(source),
                    _.s(source),
                    _.name(item, true),
                    chargesNeeded,
                    totalCharges
                )
            end,
            CannotRecharge = "You can't recharge this item.",
            CannotRechargeAnymore = function(source, item)
                return ("%s cannot be recharged anymore."):format(_.name(item))
            end,
        },
        SpendPower = function(source, item, chargesNeeded, totalCharges)
            return ("%s spend%s %s recharge powers. (Total:%s)"):format(
                _.name(source),
                _.s(source),
                chargesNeeded,
                totalCharges
            )
        end,
        Success = function(source, item, amount)
            return ("%s %s recharged by %s."):format(_.name(item), _.is(item), amount)
        end,
        Failure = {
            Explodes = function(source, item)
                return ("%s explode%s."):format(_.name(item), _.s(item))
            end,
            FailToRecharge = function(source, item)
                return ("%s fail%s to recharge %s."):format(_.name(source), _.s(source), _.name(item))
            end,
        },
    },
}
