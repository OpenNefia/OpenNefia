Elona.Quest = {
    Completed = "You have completed the quest!",
    CompletedTakenFrom = function(clientName)
        return ("You have completed the quest taken from %s."):format(clientName)
    end,
    FailedTakenFrom = function(clientName)
        return ("You have failed the quest taken from %s."):format(clientName)
    end,

    Deadline = {
        NoDeadline = "-",
        Days = function(days)
            return ("%sd"):format(days)
        end,
    },

    Rewards = {
        And = "and",
        Comma = ",",
        GoldPieces = function(amount)
            return ("%s gold pieces"):format(amount)
        end,
        Nothing = "nothing",
    },

    Dialog = {
        Choices = {
            About = "About the work.",
        },

        About = {
            Choices = {
                Take = "I will take the job.",
                Leave = "Not now.",
            },
        },
        TooManyUnfinished = "Hey, you've got quite a few unfinished contracts. See me again when you have finished them.",
        Accept = "Thanks. I'm counting on you.",
        Complete = {
            DoneWell = "You've done well. Thanks. Here's your reward.",
            TakeReward = "",
        },
    },

    Board = {
        Title = "Notice Board",
        Difficulty = {
            Star = "$",
            Counter = function(starCount)
                return ("$ x %s"):format(starCount)
            end,
        },

        NoNewNotices = "It seems there are no new notices.",
        PromptMeetClient = "Do you want to meet the client?",
    },

    Types = {
        Supply = {
            Variants = {
                {
                    Name = "Birthday.",
                    Description = function(player, speaker, params)
                        return (
                            "I want to give my kid %s as a birthday present. If you can send me this item, I'll pay you %s in exchange."
                        ):format(params.objective, params.reward)
                    end,
                },
            },
            Detail = function(params)
                return ("Give %s to the client."):format(params.objective)
            end,
            Dialog = {
                Give = function(item)
                    return ("Here is %s you asked."):format(_.name(item, nil, 1))
                end,
            },
        },
    },
}
