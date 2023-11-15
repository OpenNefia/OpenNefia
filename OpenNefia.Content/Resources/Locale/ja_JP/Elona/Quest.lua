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
            Deliver = "配達物を渡す",
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
        Name = "掲載されている依頼",
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
        Deliver = {
            Dialog = {
                BackpackIsFull = function(speaker)
                    return ("どうやらバックパックが一杯のよう%s持ち物を整理してまた来て%s"):format(
                        _.da(speaker),
                        _.kure(speaker)
                    )
                end,
                Accept = function(speaker)
                    return ("これが依頼の品物%s期限には十分気をつけて%s"):format(
                        _.da(speaker),
                        _.kure(speaker)
                    )
                end,
            },
            Detail = function(params)
                return ("%sに住む%sに%sを配達"):format(
                    params.targetMapName,
                    params.targetCharaName,
                    params.itemName
                )
            end,

            Categories = {
                Elona = {
                    ItemCatSpellbook = {
                        Variants = {
                            {
                                Name = "見習い魔術師の要望",
                                Description = function(player, speaker, params)
                                    return (
                                        "%sの%sという者が、魔法を勉強しているそう%s。%sを無事届けてくれれば、報酬として%sを払%s。"
                                    ):format(
                                        params.targetMapName,
                                        params.targetCharaName,
                                        _.da(speaker, 4),
                                        params.itemName,
                                        params.reward,
                                        _.u(speaker, 4)
                                    )
                                end,
                            },
                            {
                                Name = "本の返却",
                                Description = function(player, speaker, params)
                                    return (
                                        "%sという%sに住む知り合いに、借りていた%sを届けてくれない%s。報酬は%s%s。"
                                    ):format(
                                        params.targetCharaName,
                                        params.targetMapName,
                                        params.itemName,
                                        _.kana(speaker, 4),
                                        params.reward,
                                        _.da(speaker, 4)
                                    )
                                end,
                            },
                            {
                                Name = "珍しい本の配送",
                                Description = function(player, speaker, params)
                                    return (
                                        "%sという本が最近手に入ったので、前から欲しがっていた%sにプレゼントしたい。%sを手間賃として払うので、%sまで行って届けてくれない%s？"
                                    ):format(
                                        params.itemName,
                                        params.targetCharaName,
                                        params.reward,
                                        params.targetMapName,
                                        _.kana(speaker, 4)
                                    )
                                end,
                            },
                        },
                    },
                    ItemCatFurniture = {
                        Variants = {
                            {
                                Name = "家具の配達",
                                Description = function(player, speaker, params)
                                    return (
                                        "%sを配達して%s。期限内に無事に届ければ、配達先で報酬の%sを払%s。"
                                    ):format(
                                        params.itemName,
                                        _.kure(speaker, 4),
                                        params.reward,
                                        _.u(speaker, 4)
                                    )
                                end,
                            },
                            {
                                Name = "お祝いの品",
                                Description = function(player, speaker, params)
                                    return (
                                        "友達の%sが%sに家を建てたので、お祝いに%sをプレゼントしようと思%s。%sで届けてくれない%s？"
                                    ):format(
                                        params.targetCharaName,
                                        params.targetMapName,
                                        params.itemName,
                                        _.u(speaker, 4),
                                        params.reward,
                                        _.kana(speaker, 4)
                                    )
                                end,
                            },
                        },
                    },
                    ItemCatJunk = {
                        Variants = {
                            {
                                Name = "珍品の配達",
                                Description = function(player, speaker, params)
                                    return (
                                        "配達の依頼%s。なんに使うのか知らない%s、%sが%sを買い取りたいそう%s。%sまで配達すれば%sを払%s。"
                                    ):format(
                                        _.da(speaker, 4),
                                        ga(speaker, 4),
                                        params.targetCharaName,
                                        params.itemName,
                                        _.da(speaker, 4),
                                        params.targetMapName,
                                        params.reward,
                                        _.u(speaker, 4)
                                    )
                                end,
                            },
                            {
                                Name = "廃品回収",
                                Description = function(player, speaker, params)
                                    return (
                                        "知っていた%s。%sに住む%sが廃品を回収しているらしい%s。%sを送ろうと思うが、面倒なので%sの手間賃で代わりに持っていって%s。"
                                    ):format(
                                        _.kana(speaker, 4),
                                        params.targetMapName,
                                        params.targetCharaName,
                                        _.yo(speaker, 4),
                                        params.itemName,
                                        params.reward,
                                        _.kure(speaker, 4)
                                    )
                                end,
                            },
                        },
                    },
                    ItemCatOre = {
                        Variants = {
                            {
                                Name = "鉱石収集家に届け物",
                                Description = function(player, speaker, params)
                                    return (
                                        "%sに%sという鉱石収集家がいる%s。この%sを届けてもらえない%s。報酬は%s%s。"
                                    ):format(
                                        params.targetMapName,
                                        params.targetCharaName,
                                        _.noda(speaker, 4),
                                        params.itemName,
                                        _.kana(speaker, 4),
                                        params.reward,
                                        _.da(speaker, 4)
                                    )
                                end,
                            },
                            {
                                Name = "石材の配送",
                                Description = function(player, speaker, params)
                                    return (
                                        "%sで、%sを素材に、彫刻コンテストが開かれるそう%s。責任者の%sまで、材料を届けてくれる人を探している%s。お礼には%sを用意してい%s。"
                                    ):format(
                                        params.targetMapName,
                                        params.itemName,
                                        _.da(speaker, 4),
                                        params.targetCharaName,
                                        _.noda(speaker, 4),
                                        params.reward,
                                        _.ru(speaker, 4)
                                    )
                                end,
                            },
                            {
                                Name = "鉱石のプレゼント",
                                Description = function(player, speaker, params)
                                    return (
                                        "長年の友好の証として、%sに%sを送ろうと思ってい%s。%sで%sまで運んでもらえない%s？"
                                    ):format(
                                        params.targetCharaName,
                                        params.itemName,
                                        _.ru(speaker, 4),
                                        params.reward,
                                        params.targetMapName,
                                        _.kana(speaker, 4)
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
                    Name = "自分へのプレゼント",
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
                    Name = "子供の誕生日",
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
                    Name = "研究の素材",
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
                    Name = "コレクターの要望",
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
                    Name = "アイテムの納入",
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
                            params.itemName,
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
