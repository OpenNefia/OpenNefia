Elona.Dialog.Innkeeper = {
    Choices = {
        BuyMeal = "用餐",
        GoToShelter = "进入避难所",
    },
    BuyMeal = {
        NotHungry = function(speaker)
            return ("似乎不太饿%s"):format(_.yo(speaker))
        end,
        HereYouAre = function(speaker)
            return ("%s，请"):format(_.dozo(speaker))
        end,
        Results = { "相当美味。", "还不错。", "你拍了拍肚子。" },
    },
    GoToShelter = function(speaker)
        return ("恶劣天气时，避难所免费开放%s赶紧避难%s"):format(
            _.noda(speaker),
            _.kure(speaker)
        )
    end,
}