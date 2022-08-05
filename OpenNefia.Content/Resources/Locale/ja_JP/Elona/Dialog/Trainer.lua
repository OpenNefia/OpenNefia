Elona.Dialog.Trainer = {
    Choices = {
        Train = "訓練したい",
        Learn = "新しい能力を覚えたい",
        GoBack = "やめる",
    },
    ComeAgain = function(speaker)
        return ("訓練が必要なときは、声をかけて%s"):format(_.kure(speaker))
    end,

    Train = {
        Choices = {
            Confirm = "訓練する",
        },
        Cost = function(speaker, skillName, cost)
            return ("%sの能力を訓練するには%s platかかるけどいい%s"):format(
                skillName,
                cost,
                _.kana(speaker, 1)
            )
        end,
        Finish = function(speaker)
            return ("訓練は完了し%s潜在能力が伸びているはずなので、後は自分で鍛えて%s"):format(
                _.ta(speaker),
                _.kure(speaker)
            )
        end,
    },

    Learn = {
        Choices = {
            Confirm = "習得する",
        },
        Cost = function(speaker, skillName, cost)
            return ("%sの能力を習得するには%s platかかるけどいい%s"):format(
                skillName,
                cost,
                _.kana(speaker, 1)
            )
        end,
        Finish = function(speaker)
            return ("可能な限りの知識は教え%s後は存分に訓練して%s"):format(
                _.ta(speaker),
                _.kure(speaker)
            )
        end,
    },
}
