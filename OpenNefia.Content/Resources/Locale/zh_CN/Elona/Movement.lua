Elona.Movement = {
    Displace = {
        Text = function(source, target)
            return ("%s交换了位置。"):format(_.name(target))
        end,
        Dialog = { "「哎呀，对不起」", "「小心点」" },
        InterruptActivity = function(source, target)
            return ("%s盯着%s。"):format(_.name(target), _.name(source))
        end,
    },
}