Elona.Quest = {
    Completed = "You have completed the quest!",
    CompletedTakenFrom = function(clientName)
        return ("You have completed the quest taken from %s."):format(clientName)
    end,
    FailedTakenFrom = function(clientName)
        return ("You have failed the quest taken from %s."):format(clientName)
    end,
    MinutesLeft = function(minutesLeft)
        return ("%s min left for the quest."):format(minutesLeft)
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
            Give = function(item)
                return ("Here is %s you asked."):format(_.name(item, nil, 1))
            end,
            Deliver = "Here's your delivery.",
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
        Deliver = {
            Dialog = {
                BackpackIsFull = "It seems your backpack is already full. Come see me again when you're ready.",
                Accept = "Here's the package. Be aware of the deadline. I don't want to report you to the guards.",
            },
            Detail = function(params)
                return ("Deliver %s to %s who lives in %s."):format(
                    params.itemName,
                    params.targetCharaName,
                    params.targetMapName
                )
            end,
            Fail = "You commit a serious crime!",

            Categories = {
                Elona = {
                    ItemCatSpellbook = {
                        Variants = {
                            {
                                Name = "Book delivery.",
                                Description = function(player, speaker, params)
                                    return ("Can you take %s to a person named %s who lives in %s? I'll pay you %s."):format(
                                        params.itemName,
                                        params.targetCharaName,
                                        params.targetMapName,
                                        params.reward
                                    )
                                end,
                            },
                        },
                    },
                    ItemCatFurniture = {
                        Variants = {
                            {
                                Name = "A present.",
                                Description = function(player, speaker, params)
                                    return (
                                        "My uncle %s has built a house in %s and I'm planning to send %s as a gift. I have %s in reward."
                                    ):format(
                                        params.targetCharaName,
                                        params.targetMapName,
                                        params.itemName,
                                        params.reward
                                    )
                                end,
                            },
                        },
                    },
                    ItemCatJunk = {
                        Variants = {
                            {
                                Name = "Ecologist.",
                                Description = function(player, speaker, params)
                                    return (
                                        "My friend in %s is collecting waste materials. The name is %s. If you plan to visit %s, could you hand him %s? I'll pay you %s."
                                    ):format(
                                        params.targetMapName,
                                        params.targetCharaName,
                                        params.targetMapName,
                                        params.itemName,
                                        params.reward
                                    )
                                end,
                            },
                        },
                    },
                    ItemCatOre = {
                        Variants = {
                            {
                                Name = "A small token.",
                                Description = function(player, speaker, params)
                                    return (
                                        "As a token of our long lasting friendship, I decided to give %s to %s who lives in %s. I'll arrange %s for your reward."
                                    ):format(
                                        params.itemName,
                                        params.targetCharaName,
                                        params.targetMapName,
                                        params.reward
                                    )
                                end,
                            },
                        },
                    },
                },
            },
        },

        Supply = {
            Variants = {
                {
                    Name = "Birthday.",
                    Description = function(player, speaker, params)
                        return (
                            "I want to give my kid %s as a birthday present. If you can send me this item, I'll pay you %s in exchange."
                        ):format(params.itemName, params.reward)
                    end,
                },
            },
            Detail = function(params)
                return ("Give %s to the client."):format(params.itemName)
            end,
        },

        Collect = {
            TargetIn = function(mapName)
                return ("the target in %s"):format(mapName)
            end,
            Detail = function(params)
                return ("Acquire %s from %s for the client."):format(params.itemName, params.targetName)
            end,

            Variants = {
                {
                    Name = "I want it!",
                    Description = function(player, speaker, params)
                        return (
                            "Have you seen %s's %s? I want it! I want it! Get it for me by fair means or foul! I'll give you %s."
                        ):format(params.targetName, params.itemName, params.reward)
                    end,
                },
            },
        },

        Escort = {
            Detail = function(params)
                return ("Escort the client to %s."):format(params.targetMapName)
            end,
            CaughtByAssassins = "You are caught by assassins. You have to protect your client.",
            Complete = {
                Message = "You complete the escort.",
                Dialog = "We made it! Thank you!",
            },
            Fail = {
                Reason = {
                    FailedToProtect = "You have failed to protect the client.",
                    LeftYourClient = "You left your client.",
                },
                Dialog = {
                    Protect = _.quote "Hey, the assassins are killing me.",
                    Deadline = function(entity)
                        return (
                            "\"I missed the deadline. I don't have a right to live anymore.\" %s pours a bottole of molotov cocktail over %s."
                        ):format(_.name(entity), _.himself(entity))
                    end,
                    Poison = _.quote "Poison! P-P-Poison in my vein!!",
                },
            },

            Variants = {
                Protect = {
                    {
                        Name = "Beauty and the beast",
                        Description = function(player, speaker, params)
                            return (
                                "Such great beauty is a sin...My girl friend is followed by her ex-lover and needs an escort. If you safely bring her to %s, I'll give you %s. Please, protect her from the beast."
                            ):format(params.targetMapName, params.reward)
                        end,
                    },
                },
                Poison = {
                    {
                        Name = "Before it's too late.",
                        Description = function(player, speaker, params)
                            return (
                                "Terrible thing happened! My dad is affected by a deadly poison. Hurry! Please take him to his doctor in %s. I'll let you have %s if you sucssed!"
                            ):format(params.targetMapName, params.reward)
                        end,
                    },
                },
                Deadline = {
                    {
                        Name = "Escort needed.",
                        Description = function(player, speaker, params)
                            return (
                                "We have this client secretly heading to %s for certain reasons. We offer you %s if you succeed in escorting this person."
                            ):format(params.targetMapName, params.reward)
                        end,
                    },
                },
            },
        },

        Party = {
            Detail = function(params)
                return ("Gather %s."):format(params.requiredPoints)
            end,
            Points = function(points)
                return ("%s points"):format(points)
            end,

            Dialog = {
                Accept = "Alright, I'll take you to the party now.",
                GiveMusicTickets = "The party was terrific! I'll give you these tickets as an extra bonus.",
            },

            Event = {
                OnMapEnter = function(minutes, points)
                    return ("You have to warm up the party within %s minutes. Your target score is %s points."):format(
                        minutes,
                        points
                    )
                end,

                Points = "points",
                IsSatisfied = function(entity)
                    return ("%s %s satisfied."):format(_.basename(entity), _.is(entity))
                end,
                IsOver = "The party is over.",
                Complete = "People had a hell of a good time!",
                Fail = "The party turned out to be a big flop...",
                FinalScore = function(points)
                    return ("Your final score is %s points!"):format(points)
                end,
                TotalBonus = function(percent)
                    return ("(Total Bonus:%s%%)"):format(percent)
                end,
            },

            Variants = {
                {
                    Name = "Party time!",
                    Description = function(player, speaker, params)
                        return (
                            "I'm throwing a big party today. Many celebrities are going to attend the party so I need someone to keep them entertained. If you successfully gather %s, I'll give you a platinum coin. You'll surely be earning tons of tips while you work, too."
                        ):format(params.requiredPoints)
                    end,
                },
            },
        },
    },
}
