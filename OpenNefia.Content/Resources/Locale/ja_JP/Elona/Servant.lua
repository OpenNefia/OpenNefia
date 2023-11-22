Elona.Servant = {
    Count = function(curServants, maxServants)
        return ("現在%s人の滞在者がいる(最大%s人) "):format(curServants, maxServants)
    end,

    Hire = {
        TooManyGuests = "家はすでに人であふれかえっている。",
        Who = "誰を雇用する？",
        NotEnoughMoney = "お金が足りない…",
        YouHire = function(entity)
            return ("%sを家に迎えた。"):format(_.basename(entity))
        end,

        Topic = {
            InitCost = "雇用費(給料)",
        },
    },
}
