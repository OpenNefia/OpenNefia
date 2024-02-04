Elona.Dialog.Trainer = {
    Choices = {
        Train = "想要训练",
        Learn = "想要学习新能力",
        GoBack = "离开",
    },
    ComeAgain = function(speaker)
        return ("需要训练的时候，可以找%s"):format(_.kure(speaker))
    end,

    Train = {
        Choices = {
            Confirm = "进行训练",
        },
        Cost = function(speaker, skillName, cost)
            return ("要训练%s的能力需要%s白银，可以吗%s"):format(
                skillName,
                cost,
                _.kana(speaker, 1)
            )
        end,
        Finish = function(speaker)
            return ("训练完成%s潜能应该已经提升了，之后就自己努力%s"):format(
                _.ta(speaker),
                _.kure(speaker)
            )
        end,
    },

    Learn = {
        Choices = {
            Confirm = "进行学习",
        },
        Cost = function(speaker, skillName, cost)
            return ("要学习%s的能力需要%s白银，可以吗%s"):format(
                skillName,
                cost,
                _.kana(speaker, 1)
            )
        end,
        Finish = function(speaker)
            return ("我会尽可能地教授知识%s之后就尽情地训练%s"):format(
                _.ta(speaker),
                _.kure(speaker)
            )
        end,
    },
}