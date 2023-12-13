Elona.Quest = {
    Completed = "クエストを達成した！",
    CompletedTakenFrom = function(clientName)
        return ("%sから受けた依頼を完了した。"):format(clientName)
    end,
    FailedTakenFrom = function(clientName)
        return ("%sから受けた依頼は失敗に終わった。"):format(clientName)
    end,
    MinutesLeft = function(minutesLeft)
        return ("クエスト[残り%s分]"):format(minutesLeft)
    end,
    AboutToAbandon = "注意！現在のクエストは失敗に終わってしまう。",
    LeftYourClient = "あなたはクライアントを置き去りにした。",

    Eliminate = {
        Complete = "エリアを制圧した！",
        TargetsRemaining = function(count)
            return ("[殲滅依頼]残り%s体] "):format(count)
        end,
    },

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
        Hunt = {
            Dialog = {
                Accept = function(speaker, player)
                    return ("では、早速案内するので、モンスターを一匹残らず退治して%s"):format(
                        _.kure(speaker)
                    )
                end,
            },
            Detail = "全ての敵の殲滅",

            Variants = {
                {
                    Name = "森の清浄化",
                    Description = function(player, speaker, params)
                        return (
                            "森が危険になってい%s。近辺の森にモンスターが発生したよう%s。%sを出すので、誰か退治して%s。"
                        ):format(
                            _.ru(speaker, 4),
                            _.da(speaker, 4),
                            params.reward,
                            _.kure(speaker, 4)
                        )
                    end,
                },
                {
                    Name = "魔物退治",
                    Description = function(player, speaker, params)
                        return (
                            "%sを報酬として払%s。ある場所の魔物どもを退治してもらいたい%s。"
                        ):format(params.reward, _.u(speaker, 4), _.noda(speaker, 4))
                    end,
                },
                {
                    Name = "家の周りのモンスター",
                    Description = function(player, speaker, params)
                        return (
                            "自宅の近辺にモンスターが出没して困っている%s。退治してくれるなら、報酬として%sを払%s。"
                        ):format(_.noda(speaker, 4), params.reward, _.u(speaker, 4))
                    end,
                },
            },
        },
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
            Fail = "あなたは重大な罪を犯した!",

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
                                        _.ga(speaker, 4),
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

        Escort = {
            Detail = function(params)
                return ("クライアントを%sまで護衛"):format(params.targetMapName)
            end,
            CaughtByAssassins = "暗殺者につかまった。あなたはクライアントを守らなければならない。",
            Complete = {
                Message = "あなたは無事に護衛の任務を終えた。",
                Dialog = function(entity)
                    return ("無事に到着できてほっとした%s%s"):format(_.yo(entity), _.thanks(entity, 2))
                end,
            },
            Fail = {
                Reason = {
                    FailedToProtect = "あなたは護衛の任務を果たせなかった。",
                },
                Dialog = {
                    Protect = _.quote "おい、暗殺者が私の後ろにいるぞ",
                    Deadline = function(entity)
                        return ("「時間切れだ。こうなったら…」%sは火をかぶった。"):format(
                            _.name(entity)
                        )
                    end,
                    Poison = _.quote "毒が、毒がー！",
                },
            },

            Type = {
                Protect = {
                    {
                        Name = "使者の護衛",
                        Description = function(player, speaker, params)
                            return (
                                "あまり大きな声ではいえない%s、重要な使者を%sまで無事届ける必要が%s。報酬は%s%s。%sの首がかかってい%s。絶対にしくじらないで%s！"
                            ):format(
                                _.ga(speaker, 4),
                                params.targetMapName,
                                _.aru(speaker, 4),
                                params.reward,
                                _.da(speaker, 4),
                                _.ore(speaker, 4),
                                _.ru(speaker, 4),
                                _.kure(speaker, 4)
                            )
                        end,
                    },
                    {
                        Name = "美しすぎる人",
                        Description = function(player, speaker, params)
                            return (
                                "美しすぎるのも罪なもの%s。%sの恋人が、以前交際を断った変質者に狙われて困っている%s。危険な旅になるだろう%s、%sと引き換えに、%sまで無事に護衛して%s。"
                            ):format(
                                _.dana(speaker, 4),
                                _.ore(speaker, 4),
                                _.noda(speaker, 4),
                                _.ga(speaker, 4),
                                params.reward,
                                params.targetMapName,
                                _.kure(speaker, 4)
                            )
                        end,
                    },
                    {
                        Name = "暗殺を防げ",
                        Description = function(player, speaker, params)
                            return (
                                "あの方が、命を狙われているのは知っている%s？これは危険な依頼%s。%sに到着するまで護衛を全うしてくれれば、報酬に%sを出す%s。"
                            ):format(
                                _.daro(speaker, 4),
                                _.da(speaker, 4),
                                params.targetMapName,
                                params.reward,
                                _.yo(speaker, 4)
                            )
                        end,
                    },
                },
                Poison = {
                    {
                        Name = "急ぎの護衛",
                        Description = function(player, speaker, params)
                            return (
                                "とにかく大至急%sまで送ってもらいたい人がいる%s。報酬は%s。くれぐれも期限を過ぎないよう注意して%s。"
                            ):format(
                                params.targetMapName,
                                _.noda(speaker, 4),
                                params.reward,
                                _.kure(speaker, 4)
                            )
                        end,
                    },
                    {
                        Name = "死ぬ前に一度だけ",
                        Description = function(player, speaker, params)
                            return (
                                "言わなくてもわかっているん%s？そう、%sの最愛の人がもうすぐ病気で死んでしまう%s。%sに、最後に思い出の街%sに連れて行ってやりたい%s！%sで引き受けて%s。"
                            ):format(
                                _.daro(speaker, 4),
                                _.ore(speaker, 4),
                                _.noda(speaker, 4),
                                params.targetMapName,
                                params.targetMapName,
                                _.noda(speaker, 4),
                                params.reward,
                                _.kure(speaker, 4)
                            )
                        end,
                    },
                    {
                        Name = "手遅れにならないうちに",
                        Description = function(player, speaker, params)
                            return (
                                "大変%s！%sの親父がもの凄い猛毒に犯されてしまった%s！%s、%sに住むといわれる名医まで、大至急連れて行って%s！%sの全財産ともいうべき%sを謝礼に用意して%s！"
                            ):format(
                                _.da(speaker, 4),
                                _.ore(speaker, 4),
                                _.noda(speaker, 4),
                                _.tanomu(speaker, 4),
                                params.targetMapName,
                                _.kure(speaker, 4),
                                _.ore(speaker, 4),
                                params.reward,
                                _.aru(speaker, 4)
                            )
                        end,
                    },
                },
                Deadline = {
                    {
                        Name = "護衛求む！",
                        Description = function(player, speaker, params)
                            return (
                                "わけあって%sまで護衛して欲しいという方がいる%s。特に狙われるようなこともないと思う%s、成功すれば%sを払う%s。冒険者にとっては、簡単な依頼%s？"
                            ):format(
                                params.targetMapName,
                                _.noda(speaker, 4),
                                _.ga(speaker, 4),
                                params.reward,
                                _.yo(speaker, 4),
                                _.dana(speaker, 4)
                            )
                        end,
                    },
                    {
                        Name = "観光客の案内",
                        Description = function(player, speaker, params)
                            return (
                                "何だか変な観光客に付きまとわれて、困っている%s！手間賃として%sを払うから、やっこさんを%sあたりまで案内してやって%s。"
                            ):format(
                                _.noda(speaker, 4),
                                params.reward,
                                params.targetMapName,
                                _.kure(speaker, 4)
                            )
                        end,
                    },
                    {
                        Name = "簡単な護衛",
                        Description = function(player, speaker, params)
                            return (
                                "%sの大の仲良しの親戚が%sに行きたがってい%s。生憎今は手を離せないので、期限内に無事送ってもらえれば、お礼に%sを払う%s。"
                            ):format(
                                _.ore(speaker, 4),
                                params.targetMapName,
                                _.ru(speaker, 4),
                                params.reward,
                                _.yo(speaker, 4)
                            )
                        end,
                    },
                },
            },
        },

        HuntEX = {
            Detail = "全ての敵の殲滅",

            Variants = {
                {
                    Name = "街の危機",
                    Description = function(player, speaker, params)
                        return (
                            "もう噂を耳にしたかもしれない%s、%sの亜種レベル%s相当が街の各地に出没してい%s。このままでは%sたちの平和も長くは続かない%s。%s、奴らを退治して%s。街を代表して報酬に%sを用意し%s。"
                        ):format(
                            _.ga(speaker, 4),
                            params.enemyName,
                            params.enemyLevel,
                            _.ru(speaker, 4),
                            _.ore(speaker, 4),
                            _.daro(speaker, 4),
                            _.tanomu(speaker, 4),
                            _.kure(speaker, 4),
                            params.reward,
                            _.ta(speaker, 4)
                        )
                    end,
                },
                {
                    Name = "井戸の呪い",
                    Description = function(player, speaker, params)
                        return (
                            "どこかの馬鹿が井戸におかしな液体を混ぜやがった%s！おかげで街の中を変異したレベル%s相当の%sが徘徊してい%s。役所に頼んで、なんとか報酬の%sは集めた%s。早くなんとかして%s！"
                        ):format(
                            _.yo(speaker, 4),
                            params.enemyLevel,
                            params.enemyName,
                            _.ru(speaker, 4),
                            params.reward,
                            _.yo(speaker, 4),
                            _.kure(speaker, 4)
                        )
                    end,
                },
                {
                    Name = "エーテル変異体",
                    Description = function(player, speaker, params)
                        return (
                            "大変%s！大変%s！%sの隣の家の一家全員が、エーテル病で%sに変異してしまった%s！見たところ、強さはレベル%sぐらいじゃない%s？ともかく、すぐに退治して%s。報酬は%s払%s。"
                        ):format(
                            _.da(speaker, 4),
                            _.da(speaker, 4),
                            _.ore(speaker, 4),
                            params.enemyName,
                            _.noda(speaker, 4),
                            params.enemyLevel,
                            _.kana(speaker, 4),
                            _.kure(speaker, 4),
                            params.reward,
                            _.u(speaker, 4)
                        )
                    end,
                },
            },
        },

        Conquer = {
            UnknownMonster = "正体不明の存在",
            Detail = function(params)
                return ("%sの討伐"):format(params.enemyName)
            end,

            Event = {
                OnMapEnter = function(enemyName, timeLimitMinutes)
                    return ("%s分以内に、%sを討伐しなければならない。"):format(
                        timeLimitMinutes,
                        enemyName
                    )
                end,

                Complete = "討伐に成功した！",
                Fail = "討伐に失敗した…",
            },

            Variants = {
                {
                    Name = "討伐の依頼",
                    Description = function(player, speaker, params)
                        return (
                            "熟練の冒険者にだけ頼める依頼%s。%sの変種が街に向かっているのが確認された%s。討伐すれば報酬に%sを与え%s。これは平凡な依頼ではない%s。この怪物の強さは、少なくともレベル%sは下らない%s。"
                        ):format(
                            _.da(speaker, 4),
                            params.enemyName,
                            _.noda(speaker, 4),
                            params.reward,
                            _.ru(speaker, 4),
                            _.yo(speaker, 4),
                            params.enemyLevel,
                            _.daro(speaker, 4)
                        )
                    end,
                },
                {
                    Name = "助けて！",
                    Description = function(player, speaker, params)
                        return (
                            "ふざけて友人に変異のポーションを飲ませたら、%sになってしまった%s！レベル%sはあるモンスター%s！街の平和のためにも一刻もはやく始末して%s！お礼%s？もちろん、%sを用意し%s。%s！"
                        ):format(
                            params.enemyName,
                            _.yo(speaker, 4),
                            params.enemyLevel,
                            _.da(speaker, 4),
                            _.kure(speaker, 4),
                            _.ka(speaker, 4),
                            params.reward,
                            _.ta(speaker, 4),
                            _.tanomu(speaker, 4)
                        )
                    end,
                },
                {
                    Name = "特務指令",
                    Description = function(player, speaker, params)
                        return (
                            "脅威の芽はなるべく早く摘み取らなければならない%s。この街の中で、軍の実験体が檻から出て暴れている%s。%sを払うので討伐して%s。研究データによると、この%sはレベル%sのモンスターに匹敵する強さらしい%s。気をつけてかかって%s。"
                        ):format(
                            _.na(speaker, 4),
                            _.noda(speaker, 4),
                            params.reward,
                            _.kure(speaker, 4),
                            params.enemyName,
                            params.enemyLevel,
                            _.yo(speaker, 4),
                            _.kure(speaker, 4)
                        )
                    end,
                },
            },
        },

        Party = {
            Detail = function(params)
                return ("%sの獲得"):format(params.requiredPoints)
            end,
            Points = function(points)
                return ("%sポイント"):format(points)
            end,

            Dialog = {
                Accept = function(speaker)
                    return ("ついて来て%sパーティー会場まで案内する%s"):format(
                        _.kure(speaker),
                        _.yo(speaker)
                    )
                end,
                GiveMusicTickets = function(entity)
                    return ("予想以上の盛り上がりだったから、おまけをあげる%s"):format(
                        _.yo(entity)
                    )
                end,
            },

            Event = {
                OnMapEnter = function(minutes, points)
                    return ("%s分間の間にパーティーを盛り上げよう。目標は%sポイント。"):format(
                        minutes,
                        points
                    )
                end,

                Points = "ポイント",
                IsSatisfied = function(entity)
                    return ("%sは満足した。"):format(_.basename(entity))
                end,
                IsOver = "パーティーは終了した。",
                Complete = "パーティーは大盛況だった！",
                Fail = "パーティーはぐだぐだになった…",
                FinalScore = function(points)
                    return ("最終得点は%sポイントだった！"):format(points)
                end,
                TotalBonus = function(percent)
                    return ("(合計ボーナス:%s%%) "):format(percent)
                end,
            },

            Variants = {
                {
                    Name = "ベイベー！",
                    Description = function(player, speaker, params)
                        return (
                            "ベイベーのってる%s！イェーイ、%sは凄くハイテンション%s！%sも%sのパーティーに来て%s。報酬？そんな野暮なものはない%s！%s！芸で%s記録すればプラチナコインをプレゼントする%s！イェーイ！"
                        ):format(
                            _.kana(speaker, 4),
                            _.ore(speaker, 4),
                            _.da(speaker, 4),
                            _.kimi(speaker, 4),
                            _.ore(speaker, 4),
                            _.kure(speaker, 4),
                            _.yo(speaker, 4),
                            _.ga(speaker, 4),
                            params.requiredPoints,
                            _.yo(speaker, 4)
                        )
                    end,
                },
                {
                    Name = "セレブパーティー",
                    Description = function(player, speaker, params)
                        return (
                            "%sの名前を知らない？世間知らずの人間もいるもの%s。%sは泣く子も黙るトップセレブなの%s。近く開くパーティーの席で客を楽しませてくれる芸人を募集中%s。%sのパフォーマンスを出せたら、プラチナコインを払%s。"
                        ):format(
                            _.ore(speaker, 4),
                            _.da(speaker, 4),
                            _.ore(speaker, 4),
                            _.da(speaker, 4),
                            _.da(speaker, 4),
                            params.requiredPoints,
                            _.u(speaker, 4)
                        )
                    end,
                },
                {
                    Name = "代替芸人募集",
                    Description = function(player, speaker, params)
                        return (
                            "ああ、だれか%s、%sの代わりにパーティーで芸を披露して%s。聴衆の反応が怖くてとても舞台にあがれない%s！どうにか%s得点を稼いでくれれば、お礼にプラチナコインをあげ%s。"
                        ):format(
                            _.tanomu(speaker, 4),
                            _.ore(speaker, 4),
                            _.kure(speaker, 4),
                            _.noda(speaker, 4),
                            params.requiredPoints,
                            _.ru(speaker, 4)
                        )
                    end,
                },
            },
        },
    },
}
