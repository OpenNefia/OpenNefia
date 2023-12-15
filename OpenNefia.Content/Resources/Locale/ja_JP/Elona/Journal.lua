Elona.Journal = {
    Layer = {
        Window = {
            Title = "ジャーナル",
        },
    },

    Pages = {
        Quest = {
            MainQuest = {
                Title = "メインクエスト",
            },
            Status = {
                Job = "依頼",
                Complete = "依頼 完了",
            },
            ReportToTheClient = "あとは報告するだけだ。",
            Client = function(clientName)
                return ("依頼: %s"):format(clientName)
            end,
            Deadline = function(deadlineText)
                return ("期限: %s"):format(deadlineText)
            end,
            Detail = function(detailText)
                return ("内容: %s"):format(detailText)
            end,
            Location = function(locationName)
                return ("場所: %s"):format(locationName)
            end,
            Remaining = function(remainingTime)
                return ("残り%s"):format(remainingTime)
            end,
            Reward = function(rewardText)
                return ("報酬: %s"):format(rewardText)
            end,
        },
        TitleAndRanking = {
            Fame = function(_1)
                return ("名声: %d"):format(_1)
            end,
            Arena = function(_1, _2)
                return ("EXバトル: 勝利 %s回 最高Lv%s"):format(_1, _2)
            end,
            Deadline = function(_1)
                return ("ノルマ: %s日以内"):format(_1)
            end,
            Pay = function(_1)
                return ("給料: 約 %s gold  "):format(_1)
            end,
        },
        IncomeAndExpense = {
            Bills = {
                Title = "◆ 請求書内訳(毎月1日に発行)",
                Labor = function(_1)
                    return ("　人件費  : 約 %s gold"):format(_1)
                end,
                Maintenance = function(_1)
                    return ("　運営費  : 約 %s gold"):format(_1)
                end,
                Sum = function(_1)
                    return ("　合計　  : 約 %s gold"):format(_1)
                end,
                Tax = function(_1)
                    return ("　税金    : 約 %s gold"):format(_1)
                end,
                Unpaid = function(_1)
                    return ("現在未払いの請求書は%s枚"):format(_1)
                end,
            },
            Salary = {
                Title = "◆ 給料(毎月1日と15日に支給)",
                Sum = function(_1)
                    return ("　合計　　 : 約 %s gold"):format(_1)
                end,
            },
        },
        CompletedQuests = {
            SubQuest = "サブクエスト",
            Done = "達成",
        },
    },
}
