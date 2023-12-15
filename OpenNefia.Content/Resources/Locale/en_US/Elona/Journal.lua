Elona.Journal = {
    Layer = {
        Window = {
            Title = "Journal",
        },
    },

    Pages = {
        Quest = {
            MainQuest = {
                Title = "Main Quest",
            },
            Status = {
                Job = "Job",
                Complete = "Complete",
            },
            ReportToTheClient = "Report to the client.",
            Client = function(clientName)
                return ("Client  : %s"):format(clientName)
            end,
            Deadline = function(deadlineText)
                return ("Deadline: %s"):format(deadlineText)
            end,
            Detail = function(detailText)
                return ("Detail  : %s"):format(detailText)
            end,
            Location = function(locationName)
                return ("Location: %s"):format(locationName)
            end,
            Remaining = function(remainingTime)
                return ("%s"):format(remainingTime)
            end,
            Reward = function(rewardText)
                return ("Reward  : %s"):format(rewardText)
            end,
        },
        TitleAndRanking = {
            Fame = function(_1)
                return ("Fame: %d"):format(_1)
            end,
            Arena = function(_1, _2)
                return ("EX Arena Wins:%s  Highest Level:%s"):format(_1, _2)
            end,
            Deadline = function(_1)
                return ("Deadline: %s Days left"):format(_1)
            end,
            Pay = function(_1)
                return ("Pay: About %s gold pieces "):format(_1)
            end,
        },
        IncomeAndExpense = {
            Bills = {
                Title = "Bills  (Issued every 1st day)",
                Labor = function(_1)
                    return ("  Labor  : About %s GP"):format(_1)
                end,
                Maintenance = function(_1)
                    return ("  Maint. : About %s GP"):format(_1)
                end,
                Sum = function(_1)
                    return ("  Sum    : About %s GP"):format(_1)
                end,
                Tax = function(_1)
                    return ("  Tax    : About %s GP"):format(_1)
                end,
                Unpaid = function(_1)
                    return ("You have %s unpaid bills."):format(_1)
                end,
            },
            Salary = {
                Title = "Salary (Paid every 1st and 15th day)",
                Sum = function(_1)
                    return ("  Sum    : About %s GP"):format(_1)
                end,
            },
        },
        CompletedQuests = {
            SubQuest = "Sub Quest",
            Done = "Done",
        },
    },
}
