Elona.SandBag = {
    Interact = {
        Release = function(source, target)
            return ("%s解开了%s的绳子。"):format(_.sore_wa(source), _.name(target))
        end,
    },
    Dialog = {
        TurnStart = {
            function(entity)
                return _.quote(("再打得更狠%s"):format(_.yo(entity, 2)))
            end,
            function(entity)
                return _.quote(("做这种事，我绝不会原谅你%s"):format(_.yo(entity)))
            end,
            function(entity)
                return _.quote(("你想做什么%s"):format(_.noda(entity, 2)))
            end,
        },
        Damage = {
            _.quote "咕噜！",
            _.quote "还没完！",
            _.quote "我已经到极限了…",
            _.quote "呜呜呜",
            _.quote "啊呀",
            _.quote "啊啊呜",
        },
    },
}