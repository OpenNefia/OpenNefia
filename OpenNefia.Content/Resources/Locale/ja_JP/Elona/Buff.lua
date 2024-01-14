Elona.Buff = {
    Apply = {
        NoEffect = "しかし、効果はなかった。",
        Repelled = "ホーリーヴェイルが呪いを防いだ。",
        Resists = function(target)
            return ("%sは抵抗した。"):format(_.name(target))
        end,
    },
    Ends = function(buff)
        return ("%sの効果が切れた。"):format(_.name(buff))
    end,

    Types = {
        Speed = {
            Cursed = function(target)
                return ("%s aging process slows down."):format(_.possessive(target))
            end,
        },
        Slow = {
            Blessed = function(target)
                return ("%s%s aging process speeds up."):format(_.possessive(target))
            end,
        },
    },
}
