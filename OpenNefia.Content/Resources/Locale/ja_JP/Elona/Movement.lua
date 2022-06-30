Elona.Movement = {
    Displace = {
        Text = function(source, target)
            return ("%sと入れ替わった。"):format(_.name(target))
        end,
        Dialog = { "「おっと、ごめんよ」", "「気をつけな」" },
        InterruptActivity = function(source, target)
            return ("%sは%sを睨み付けた。"):format(_.name(target), _.name(source))
        end,
    },
}
