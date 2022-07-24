Elona.CurseState = {
    CannotBeTakenOff = function(entity)
        return ("%sは外せない。"):format(_.name(entity))
    end,
    Equipped = {
        Blessed = function(actor, target, item)
            return ("%sは何かに見守られている感じがした。"):format(_.name(target))
        end,
        Cursed = function(actor, target, item)
            return ("%sは急に寒気がしてふるえた。"):format(_.name(target))
        end,
        Doomed = function(actor, target, item)
            return ("%sは破滅への道を歩み始めた。"):format(_.name(target))
        end,
    },
}
