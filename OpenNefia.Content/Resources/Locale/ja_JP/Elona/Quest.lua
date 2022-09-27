Elona.Quest = {
    Completed = "クエストを達成した！",
    CompletedTakenFrom = function(clientName)
        return ("%sから受けた依頼を完了した。"):format(clientName)
    end,
    FailedTakenFrom = function(clientName)
        return ("%sから受けた依頼は失敗に終わった。"):format(clientName)
    end,

    Deadline = {
        NoDeadline = "即時",
        Days = function(days)
            return ("%s日"):format(days)
        end,
    },

    Rewards = {
        And = "と",
        Comma = "、",
        GoldPieces = function(amount)
            return ("金貨%s枚"):format(amount)
        end,
        Nothing = "何もない",
    },

    Dialog = {
        Choices = {
            About = "依頼について",
            Give = function(item)
                return ("%sを納入する"):format(_.name(item, nil, 1))
            end,
        },

        About = {
            Choices = {
                Take = "受諾する",
                Leave = "やめる",
            },
        },
        TooManyUnfinished = function(speaker)
            return ("未完了の依頼が多すぎじゃない%sこの仕事は、安心してまかせられない%s"):format(
                _.kana(speaker, 1),
                _.yo(speaker)
            )
        end,
        Accept = function(speaker)
            return ("%s期待してい%s"):format(_.thanks(speaker), _.ru(speaker))
        end,
        Complete = {
            DoneWell = function(speaker)
                return ("依頼を無事終わらせたよう%s%s"):format(_.dana(speaker), _.thanks(speaker, 2))
            end,
            TakeReward = function(speaker, rewardText)
                return ("報酬の%sを受けとって%s"):format(rewardText, _.kure(speaker))
            end,
        },
    },

    Board = {
        Title = "掲載されている依頼",
        Difficulty = {
            Difficulty = "★",
            Counter = function(starCount)
                return ("★×%s"):format(starCount)
            end,
        },

        NoNewNotices = "新しい依頼は掲示されていないようだ。",
        PromptMeetClient = "依頼主に会う？",
    },

    Types = {
        Supply = {
            Variants = {
                {
                    Name = "恋人への贈り物",
                    Description = function(player, speaker, params)
                        return (
                            "恋人へのプレゼントはなにがいい%s？とりあえず報酬に%sを払うので、%sを調達してきて%s。"
                        ):format(
                            _.kana(speaker, 4),
                            params.reward,
                            params.itemName,
                            _.kure(speaker, 4)
                        )
                    end,
                },
                {
                    Title = "自分へのプレゼント",
                    Description = function(player, speaker, params)
                        return (
                            "最近ちょっといいことがあったので、自分に%sをプレゼントしたい%s。報酬は、%sでどう%s？"
                        ):format(
                            params.itemName,
                            _.noda(speaker, 4),
                            params.reward,
                            _.kana(speaker, 4)
                        )
                    end,
                },
                {
                    Title = "子供の誕生日",
                    Description = function(player, speaker, params)
                        return (
                            "報酬は%sぐらいが妥当%s。子供の誕生日プレゼントに%sがいる%s。"
                        ):format(
                            params.reward,
                            _.kana(speaker, 4),
                            params.itemName,
                            _.noda(speaker, 4)
                        )
                    end,
                },
                {
                    Title = "研究の素材",
                    Description = function(player, speaker, params)
                        return (
                            "%sの研究をしてい%s。研究用のストックが尽きたので、%sで調達して来てもらえない%s？"
                        ):format(
                            params.itemName,
                            _.ru(speaker, 4),
                            params.reward,
                            _.kana(speaker, 4)
                        )
                    end,
                },
                {
                    Title = "コレクターの要望",
                    Description = function(player, speaker, params)
                        return (
                            "%sのコレクションに%sが必要%s。どうか%sの報酬で依頼を受けて%s！"
                        ):format(
                            _.ore(speaker, 4),
                            params.itemName,
                            _.da(speaker, 4),
                            params.reward,
                            _.kure(speaker, 4)
                        )
                    end,
                },
                {
                    Title = "アイテムの納入",
                    Description = function(player, speaker, params)
                        return (
                            "ちょっとした用事で、%sが必要になった%s。期限内に納入してくれれば%sを払%s。"
                        ):format(
                            params.itemName,
                            _.noda(speaker, 4),
                            params.reward,
                            _.u(speaker, 4)
                        )
                    end,
                },
            },
            Detail = function(params)
                return ("%sの納入"):format(params.itemName)
            end,
        },

        Collect = {
            TargetIn = function(mapName)
                return ("%sに住む人物"):format(mapName)
            end,
            Detail = function(params)
                return ("依頼人のために%sから%sを調達"):format(params.targetName, params.itemName)
            end,

            Variants = {
                {
                    Name = "物凄く欲しい物",
                    Description = function(player, speaker, params)
                        return (
                            "%sが、最近やたらと%sを見せびらかして自慢してくる%s。%sもたまらなく%sが欲しい%s！どうにかしてブツを手に入れてくれれば、%sを払%s。手段はもちろん問わない%s。"
                        ):format(
                            params.targetName,
                            params.itemName,
                            _.noda(speaker, 4),
                            _.ore(speaker, 4),
                            params.item_name,
                            _.yo(speaker, 4),
                            params.reward,
                            _.u(speaker, 4),
                            _.yo(speaker, 4)
                        )
                    end,
                },
                {
                    Name = "狙った獲物",
                    Description = function(player, speaker, params)
                        return (
                            "%sが%sを所持しているのは知っている%s？わけあって、どうしてもこの品物が必要なの%s。うまく取り合って入手してくれれば、%sを払%s。"
                        ):format(
                            params.targetName,
                            params.itemName,
                            _.kana(speaker, 4),
                            _.da(speaker, 4),
                            params.reward,
                            _.u(speaker, 4)
                        )
                    end,
                },
            },
        },
    },
}
