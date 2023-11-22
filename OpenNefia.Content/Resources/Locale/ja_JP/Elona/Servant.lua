Elona.Servant = {
    Count = function(curServants, maxServants)
        return ("現在%s人の滞在者がいる(最大%s人) "):format(curServants, maxServants)
    end,

    Hire = {
        TooManyGuests = "家はすでに人であふれかえっている。",
        Prompt = "誰を雇用する？",
        NotEnoughMoney = "お金が足りない…",
        YouHire = function(entity)
            return ("%sを家に迎えた。"):format(_._.basename(entity))
        end,

        Topic = {
            InitCost = "雇用費(給料)",
            Wage = "給料",
        },
    },

    Move = {
        Prompt = {
            Who = "誰を移動させる？",
            Where = function(entity)
                return ("%sをどこに移動させる？"):format(_.basename(entity))
            end,
        },
        Invalid = "その場所には移動させることができない。",
        DontTouchMe = function(entity)
            return ("%s「触るな！」"):format(_.basename(entity))
        end,
        IsMoved = function(entity)
            return ("%sを移動させた。"):format(_.basename(entity))
        end,
    },
}
