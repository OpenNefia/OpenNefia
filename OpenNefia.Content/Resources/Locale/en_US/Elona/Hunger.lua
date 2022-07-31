Elona.Hunger = {
    Anorexia = {
        Develops = function(_1)
            return ("%s develop%s anorexia."):format(_.name(_1), _.s(_1))
        end,
        RecoversFrom = function(_1)
            return ("%s manage%s to recover from anorexia."):format(_.name(_1), _.s(_1))
        end,
    },
    Status = {
        Hungry = { "You are getting hungry.", "You feel hungry.", "Now what shall I eat?" },
        VeryHungry = { "Your hunger makes you dizzy.", "You have to eat something NOW." },
        Starving = { "You are starving!", "You are almost dead from hunger." },
    },
    Indicator = {
        ["0"] = "Starving!",
        ["1"] = "Starving",
        ["2"] = "Hungry!",
        ["3"] = "Hungry",
        ["4"] = "Hungry",
        ["5"] = "",
        ["6"] = "",
        ["7"] = "",
        ["8"] = "",
        ["9"] = "",
        ["10"] = "Satisfied",
        ["11"] = "Satisfied!",
        ["12"] = "Bloated",
    },
    Vomits = function(_1)
        return ("%s vomit%s."):format(_.name(_1), _.s(_1))
    end,
}
