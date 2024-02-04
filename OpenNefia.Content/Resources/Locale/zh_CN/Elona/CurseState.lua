Elona.CurseState = {
    ItemName = {
        Blessed = "被祝福的",
        Cursed = "被诅咒的",
        Doomed = "不幸的",
    },
    CannotBeTakenOff = function(entity)
        return ("%s无法取下。"):format(_.name(entity))
    end,
    Equipped = {
        Blessed = function(actor, target, item)
            return ("%s感到有某种注视。"):format(_.name(target))
        end,
        Cursed = function(actor, target, item)
            return ("%s突然感到寒意并颤抖。"):format(_.name(target))
        end,
        Doomed = function(actor, target, item)
            return ("%s开始走向毁灭之路。"):format(_.name(target))
        end,
    },
}