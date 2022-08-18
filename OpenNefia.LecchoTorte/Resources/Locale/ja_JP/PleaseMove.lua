LecchoTorte.PleaseMove = {
    Dialog = {
        Choice = "そこをどいてください。",
    },

    Refuses = function(speaker, player)
        return ("%sはやんわりと断った。"):format(_.name(speaker))
    end,

    Response = {
        function(speaker, player)
            return _.quote(("これでいい%s"):format(_.kana(speaker, 3)))
        end,
        _.quote "どうぞ",
    },
}
