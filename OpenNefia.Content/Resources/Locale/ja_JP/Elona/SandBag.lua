Elona.SandBag = {
    Interact = {
        Release = function(source, target)
            return ("%s%sの縄を解いた。"):format(_.sore_wa(source), _.name(target))
        end,
    },
    Dialog = {
        TurnStart = {
            function(entity)
                return _.quote(("もっとぶって%s"):format(_.yo(entity, 2)))
            end,
            function(entity)
                return _.quote(("こんなことして、許さない%s"):format(_.yo(entity)))
            end,
            function(entity)
                return _.quote(("何をする%s"):format(_.noda(entity, 2)))
            end,
        },
        Damage = {
            _.quote "くっ！",
            _.quote "まだまだ！",
            _.quote "もう限界…",
            _.quote "うぐぐ",
            _.quote "あう",
            _.quote "ああんっ",
        },
    },
}
