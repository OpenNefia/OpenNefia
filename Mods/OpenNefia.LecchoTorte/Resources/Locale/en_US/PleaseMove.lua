LecchoTorte.PleaseMove = {
    Dialog = {
        Choice = "Please move aside.",
    },

    Refuses = function(speaker, player)
        return ("%s gently refuse%s %s request."):format(_.name(speaker), _.s(speaker), _.possessive(player))
    end,

    Response = {
        _.quote "Here you are.",
        _.quote "Go ahead.",
    },
}
