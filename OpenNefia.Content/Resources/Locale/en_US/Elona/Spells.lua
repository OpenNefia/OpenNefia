Elona.Spells = {
    Layer = {
        Window = {
            Title = "Spell",
        },

        Topic = {
            Name = "Name",
            Cost = "Cost",
            Stock = "Stock",
            Lv = "Lv",
            Chance = "Chance",
            Effect = "Effect",
        },

        Stats = {
            Power = function(power)
                return ("Power:%s"):format(power)
            end,
            TurnCounter = function(turns)
                return ("%st"):format(turns)
            end,
        },
    },
}
