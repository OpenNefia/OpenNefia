Elona.Dialog.Innkeeper = {
    Choices = {
        BuyMeal = "食事をとる",
        GoToShelter = "シェルターに入る",
    },
    BuyMeal = {
        NotHungry = function(speaker)
            return ("腹が減っているようにはみえない%s"):format(_.yo(speaker))
        end,
        HereYouAre = function(speaker)
            return ("%s"):format(_.dozo(speaker))
        end,
        Results = { "なかなか美味しかった。", "悪くない。", "あなたは舌鼓をうった。" },
    },
    GoToShelter = function(speaker)
        return ("悪天候時はシェルターを無料で開放している%sすみやかに避難して%s"):format(
            _.noda(speaker),
            _.kure(speaker)
        )
    end,
}
