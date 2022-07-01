Elona.LayHand = {
    Dialog = function(healer)
        return ("%s shout%s, \"Lay hand!\""):format(_.name(healer), _.s(healer))
    end,
    IsHealed = function(target)
        return ("%s %s healed."):format(_.name(target), _.is(target))
    end,
}
