Elona.Journal = {
    Layer = {
        Window = {
            Title = "日志",
        },
    },

    Pages = {
        Quest = {
            MainQuest = {
                Title = "主线任务",
            },
            Status = {
                Job = "任务",
                Complete = "任务 完成",
            },
            ReportToTheClient = "剩下的就是报告了。",
            Client = function(clientName)
                return ("任务委托人: %s"):format(clientName)
            end,
            Deadline = function(deadlineText)
                return ("截止日期: %s"):format(deadlineText)
            end,
            Detail = function(detailText)
                return ("内容: %s"):format(detailText)
            end,
            Location = function(locationName)
                return ("地点: %s"):format(locationName)
            end,
            Remaining = function(remainingTime)
                return ("剩余时间%s"):format(remainingTime)
            end,
            Reward = function(rewardText)
                return ("奖励: %s"):format(rewardText)
            end,
        },
        TitleAndRanking = {
            Fame = function(_1)
                return ("声望: %d"):format(_1)
            end,
            Arena = function(_1, _2)
                return ("EX战斗: 胜利%s次 最高等级%s"):format(_1, _2)
            end,
            Deadline = function(_1)
                return ("限制: %s天以内"):format(_1)
            end,
            Pay = function(_1)
                return ("薪水: 约%s 金币  "):format(_1)
            end,
        },
        IncomeAndExpense = {
            Bills = {
                Title = "◆ 账单明细(每月1日出具)",
                Labor = function(_1)
                    return (" 人工费: 约%s 金币"):format(_1)
                end,
                Maintenance = function(_1)
                    return (" 运营费: 约%s 金币"):format(_1)
                end,
                Sum = function(_1)
                    return (" 总计  : 约%s 金币"):format(_1)
                end,
                Tax = function(_1)
                    return (" 税金  : 约%s 金币"):format(_1)
                end,
                Unpaid = function(_1)
                    return ("当前未支付的账单数: %s"):format(_1)
                end,
            },
            Salary = {
                Title = "◆ 工资(每月1日和15日支付)",
                Sum = function(_1)
                    return (" 总计  : 约%s 金币"):format(_1)
                end,
            },
        },
        CompletedQuests = {
            SubQuest = "支线任务",
            Done = "已完成",
        },
    },
}