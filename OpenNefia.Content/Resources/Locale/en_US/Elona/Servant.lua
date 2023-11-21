Elona.Servant = {
    Hire = {
        TooManyGuests = "You already have too many guests in your home.",
        Who = "Who do you want to hire?",
        NotEnoughMoney = "You don't have enough money...",
        YouHire = function(entity)
            return ("You hire %s."):format(_.basename(entity))
        end,

        Topic = {
            InitCost = "Init. Cost(Wage)",
        },
    },
}
