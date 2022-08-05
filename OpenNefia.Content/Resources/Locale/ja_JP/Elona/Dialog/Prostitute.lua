Elona.Dialog.Prostitute = {
    Choices = {
        Buy = "暗い場所に移ろう",
    },
    Buy = {
        Text = function(speaker, cost)
            return ("そう%s金貨%s枚を前払いで%s"):format(_.dana(speaker), cost, _.kure(speaker))
        end,
    },
}
