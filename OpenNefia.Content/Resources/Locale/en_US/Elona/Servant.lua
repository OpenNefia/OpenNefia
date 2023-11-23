Elona.Servant = {
    Count = function(curServants, maxServants)
        return ("%s members are staying at your home (Max: %s)."):format(curServants, maxServants)
    end,

    Hire = {
        TooManyGuests = "You already have too many guests in your home.",
        Prompt = "Who do you want to hire?",
        NotEnoughMoney = "You don't have enough money...",
        YouHire = function(entity)
            return ("You hire %s."):format(_.basename(entity))
        end,

        Topic = {
            InitCost = "Init. Cost(Wage)",
            Wage = "Wage",
        },
    },

    Move = {
        Prompt = {
            Who = "Move who?",
            Where = function(entity)
                return ("Where do you want to move %s?"):format(_.basename(entity))
            end,
        },
        Invalid = "The location is invalid.",
        DontTouchMe = function(entity)
            return ("%s\"Don't touch me!\""):format(_.basename(entity))
        end,
        IsMoved = function(entity)
            return ("%s %s moved to the location."):format(_.basename(entity), _.is(entity))
        end,
    },
}
