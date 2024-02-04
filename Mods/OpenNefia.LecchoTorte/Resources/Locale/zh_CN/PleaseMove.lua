LecchoTorte.PleaseMove = {
    Dialog = {
        Choice = "请让开。",
    },
    Refuses = function(speaker, player)
        return ("%s委婉地拒绝了。"):format(_.name(speaker))
    end,

    Response = {
        function(speaker, player)
            return _.quote(("这样就可以了%s"):format(_.kana(speaker, 3)))
        end,
        _.quote "请便",
    },
}
