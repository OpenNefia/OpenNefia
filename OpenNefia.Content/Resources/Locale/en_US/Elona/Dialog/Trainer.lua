Elona.Dialog.Trainer = {
    Choices = {
        Train = "Train me.",
        Learn = "What skills can you teach me?",
        GoBack = "Never mind.",
    },
    ComeAgain = "Come see me again when you need more training.",

    Train = {
        Choices = {
            Confirm = "Train me.",
        },
        Cost = function(speaker, skillName, cost)
            return ("Training %s will cost you %s platinum pieces."):format(skillName, cost)
        end,
        Finish = "Well done. You've got more room to develop than anyone else I've ever drilled. Keep training.",
    },

    Learn = {
        Choices = {
            Confirm = "Teach me the skill.",
        },
        Cost = function(speaker, skillName, cost)
            return ("Learning %s will cost you %s platinum pieces."):format(skillName, cost)
        end,
        Finish = "I've taught you all that I know of the skill. Now develop it by yourself.",
    },
}
