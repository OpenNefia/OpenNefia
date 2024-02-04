Elona.Quest = {
    Completed = "已达成任务！",
    CompletedTakenFrom = function(clientName)
        return ("完成了从%s那里接受的任务。"):format(clientName)
    end,
    FailedTakenFrom = function(clientName)
        return ("从%s那里接受的任务以失败告终。"):format(clientName)
    end,
    MinutesLeft = function(minutesLeft)
        return ("任务[剩余%s分钟]"):format(minutesLeft)
    end,

    ReturnIsForbidden = "在承担任务期间禁止返回。仍然要返回吗？",
    AboutToAbandon = "注意！当前任务将以失败告终。",
    LeftYourClient = "你把客户留下了。",

    Eliminate = {
        Complete = "已占领区域！",
        TargetsRemaining = function(count)
            return ("[剿灭任务]剩余%s只]"):format(count)
        end,
    },

    Deadline = {
        NoDeadline = "即时",
        Days = function(days)
            return ("%s天"):format(days)
        end,
    },

    Rewards = {
        And = "和",
        Comma = "、",
        GoldPieces = function(amount)
            return ("金币%s枚"):format(amount)
        end,
        Nothing = "没有任何奖励",
    },

    Dialog = {
        Choices = {
            About = "关于任务",
            Give = function(item)
                return ("交付%s"):format(_.name(item, nil, 1))
            end,
            Deliver = "交付物品",
        },

        About = {
            Choices = {
                Take = "接受",
                Leave = "放弃",
            },
        },
        TooManyUnfinished = function(speaker)
            return ("太多未完成的任务了%s这项工作是不可靠的%s"):format(
                _.kana(speaker, 1),
                _.yo(speaker)
            )
        end,
        Accept = function(speaker)
            return ("%s期待着%s"):format(_.thanks(speaker), _.ru(speaker))
        end,
        Complete = {
            DoneWell = function(speaker)
                return ("看来你已经顺利完成了任务%s%s"):format(_.dana(speaker), _.thanks(speaker, 2))
            end,
            TakeReward = function(speaker, rewardText)
                return ("领取了%s作为酬谢%s"):format(rewardText, _.kure(speaker))
            end,
        },
    },

    Board = {
        Name = "已发布的任务",
        Difficulty = {
            Star = "★",
            Counter = function(starCount)
                return ("★×%s"):format(starCount)
            end,
        },

        NoNewNotices = "看来没有发布新任务。",
        PromptMeetClient = "要见依頼主吗？",
    },

    Types = {
        Hunt = {
            Dialog = {
                Accept = function(speaker, player)
                    return ("好吧，我会给你引导，然后你消灭所有的怪兽%s"):format(
                        _.kure(speaker)
                    )
                end,
            },
            Detail = "消灭所有敌人",

            Variants = {
                {
                    Name = "森林清爽",
                    Description = function(player, speaker, params)
                        return (
                            "森林变得危险%s附近的森林里似乎出现了怪物%s。%s准备好奖励，所以拜托你干掉他们。"
                        ):format(
                            _.ru(speaker, 4),
                            _.da(speaker, 4),
                            params.reward,
                            _.kure(speaker, 4)
                        )
                    end,
                },
                {
                    Name = "消灭魔物",
                    Description = function(player, speaker, params)
                        return (
                            "为了报偿%s。我们需要消灭某地方的魔物%s。"
                        ):format(params.reward, _.u(speaker, 4), _.noda(speaker, 4))
                    end,
                },
                {
                    Name = "家周围的怪物",
                    Description = function(player, speaker, params)
                        return (
                            "家附近的怪物已经出现%s。如果你可以消灭它，我会支付%s作为酬劳%s。"
                        ):format(_.noda(speaker, 4), params.reward, _.u(speaker, 4))
                    end,
                },
            },
        },
        ...
    },
}