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
    Vomits = function(_1)
        return ("%s vomit%s."):format(_.name(_1), _.s(_1))
    end,
}
