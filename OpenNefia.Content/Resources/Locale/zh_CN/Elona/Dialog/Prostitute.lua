Elona.Dialog.Prostitute = {
    Choices = {
        Buy = "去一个黑暗的地方",
    },
    Buy = {
        Text = function(speaker, cost)
            return ("这样%s金币%s枚预付%s"):format(_.dana(speaker), cost, _.kure(speaker))
        end,
    },
}