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
    AboutToAbandon = "Warning! You are going to abandon your current quest.",
    LeftYourClient = "You left your client.",

    Eliminate = {
        Complete = "The area is secured!",
        TargetsRemaining = function(count)
            return ("%s more to go."):format(count)
        end,
    },

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
        Hunt = {
            Dialog = {
                Accept = "Great! I'll guide you to the place, kill them all!",
            },
            Detail = "Eliminate monsters.",

            Variants = {
                {
                    Name = "Hunting.",
                    Description = function(player, speaker, params)
                        return (
                            "Filthy creatures are spawning in a forest nearby this city. I'll give you %s if you get rid of them."
                        ):format(params.reward)
                    end,
                },
            },
        },

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

        Cook = {
            Detail = function(params)
                return ("Give %s to the client."):format(params.foodName)
            end,

            Variants = {
                General = {
                    {
                        Name = "My stomach!",
                        Description = function(player, speaker, params)
                            return (
                                "My stomach growls like I'm starving to death habitually. Will you bring a piece to this beast? Maybe %s will do the job. I can give you %s as a reward."
                            ):format(params.foodName, params.reward)
                        end,
                    },
                },
                FoodType = {
                    Elona = {
                        Meat = {
                            {
                                Name = "A reception.",
                                Description = function(player, speaker, params)
                                    return (
                                        "We will be hosting this very important reception tonight. The guests must be satisfied and made to feel gorgeous. I want you to prepare %s and %s are yours."
                                    ):format(params.foodName, params.reward)
                                end,
                            },
                        },
                        Vegetable = {
                            {
                                Name = "On a diet",
                                Description = function(player, speaker, params)
                                    return (
                                        "Vegetables are essential parts of a healthy diet. Cook %s for me. Your rewards are %s."
                                    ):format(params.foodName, params.reward)
                                end,
                            },
                        },
                        Fruit = {
                            {
                                Name = "Cocktail party!",
                                Description = function(player, speaker, params)
                                    return (
                                        "Run a small errand for us and earn %s. We need a wicked relish for our cocktail party. Say, %s sounds decent."
                                    ):format(params.reward, params.foodName)
                                end,
                            },
                        },
                        Sweet = {
                            {
                                Name = "Sweet sweet.",
                                Description = function(player, speaker, params)
                                    return ("I prefer cakes and candies to alcoholic drinks. You want %s? Gimme %s!"):format(
                                        params.reward,
                                        params.foodName
                                    )
                                end,
                            },
                        },
                        Pasta = {
                            {
                                Name = "I love noodles!",
                                Description = function(player, speaker, params)
                                    return (
                                        "I love noodles! Is there anyone that hates noodles? I want to eat %s now! Rewards? Of course. %s sound good? "
                                    ):format(params.foodName, params.reward)
                                end,
                            },
                        },
                        Fish = {
                            {
                                Name = "Fussy taste.",
                                Description = function(player, speaker, params)
                                    return (
                                        "My children won't eat fish. It's killing me. I'm gonna give %s to anyone that makes %s delicious enough to sweep their fuss!"
                                    ):format(params.reward, params.foodName)
                                end,
                            },
                        },
                        Bread = {
                            {
                                Name = "Going on a picnic.",
                                Description = function(player, speaker, params)
                                    return (
                                        "First off, the rewards are no more than %s, ok? My kid needs %s for a picnic tomorrow. Please hurry."
                                    ):format(params.reward, params.foodName)
                                end,
                            },
                        },
                        Egg = {
                            {
                                Name = "A new recipe!",
                                Description = function(player, speaker, params)
                                    return (
                                        "As in the capacity of a cooking master, I'm always eager to learn a new recipe. Bring me %s and I'll pay you %s."
                                    ):format(params.foodName, params.reward)
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

        Harvest = {
            Detail = {
                Objective = function(params)
                    return ("Gather harvests weight %s."):format(params.required_weight)
                end,
                Now = function(currentWeight)
                    return ("(Now %s)"):format(currentWeight)
                end,
            },

            Dialog = {
                Accept = "Fine. I'll take you to my farm.",
                GiveExtraCoins = "I've added some extra coins since you worked really hard.",
            },

            Event = {
                OnMapEnter = function(requiredWeight, timeLimitMinutes)
                    return (
                        "To complete the quest, you have to harvest %s worth farm products and put them into the delivery chest within %s minutes."
                    ):format(requiredWeight, timeLimitMinutes)
                end,
                Put = function(item, addWeight, currentWeight, requiredWeight)
                    return ("You deliver %s. +%s Delivered(%s) Quota (%s)"):format(
                        _.name(item),
                        addWeight,
                        currentWeight,
                        requiredWeight
                    )
                end,
                Complete = "You complete the task!",
                Fail = "You fail to fulfill your task...",
            },

            ItemName = {
                Grown = function(baseName, weightClass)
                    local weightName = _.loc(("Elona.Quest.Types.Harvest.ItemName.WeightClass.%s"):format(weightClass))
                    return ("%s grown %s"):format(baseName, weightName)
                end,
                WeightClass = {
                    ["0"] = "extremely mini",
                    ["1"] = "small",
                    ["2"] = "handy",
                    ["3"] = "rather big",
                    ["4"] = "huge",
                    ["5"] = "pretty huge",
                    ["6"] = "monstrous-size",
                    ["7"] = "bigger than a man",
                    ["8"] = "legendary-size",
                    ["9"] = "heavier than an elephant",
                },
            },

            Activity = {
                Start = function(actor, item)
                    return ("%s start%s to pick %s."):format(_.name(actor), _.s(actor), _.name(item))
                end,
                Finish = function(actor, item, weight)
                    return ("%s harvest%s %s. (%s)"):format(_.name(actor), _.s(actor), _.name(item), weight)
                end,
                Sound = { "*sing*", "*pull*", "*thud*", "*rumble*", "*gasp*" },
            },

            Variants = {
                {
                    Name = "The harvest time.",
                    Description = function(player, speaker, params)
                        return (
                            "At last, the harvest time has come. It is by no means a job that I alone can handle. You get %s if you can gather grown crops weighting %s."
                        ):format(params.reward, params.requiredWeight)
                    end,
                },
            },
        },

        HuntEX = {
            Detail = "Eliminate monsters.",

            Variants = {
                {
                    Name = "Panic.",
                    Description = function(player, speaker, params)
                        return (
                            "Help! Our town is being seized by several subspecies of %s which are expected to be around level %s. Eliminate them all and I'll reward you with %s on behalf of all the citizen."
                        ):format(params.enemyName, params.enemyLevel, params.reward)
                    end,
                },
            },
        },

        Conquer = {
            UnknownMonster = "unknown monster",
            Detail = function(params)
                return ("Slay %s."):format(params.enemyName)
            end,

            Event = {
                OnMapEnter = function(enemyName, timeLimitMinutes)
                    return ("You have to slay %s within %s minutes."):format(enemyName, timeLimitMinutes)
                end,

                Complete = "You successfully slay the target.",
                Fail = "You failed to slay the target...",
            },

            Variants = {
                {
                    Name = "Challenge",
                    Description = function(player, speaker, params)
                        return (
                            "Only experienced adventurers should take this task. An unique mutant of %s has been sighted near the town. Slay this monster and we'll give you %s. This is no ordinary mission. The monster's level is expected to be around %s."
                        ):format(params.enemyName, params.reward, params.enemyLevel)
                    end,
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
