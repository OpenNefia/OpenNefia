Elona.Dialog.Villager = {
    Choices = {
        Talk = "话想说",
        Trade = "物品交换",
        Sex = "不想尝试一下吗？",
    },

    Talk = {
        Default = {
            function(npc)
                return ("%s？ …感觉有点熟%s。"):format(_.alias(_.player()), _.aru(npc))
            end,
            function(npc)
                return ("无聊%s。"):format(_.da(npc))
            end,
            function(npc)
                return ("嗯？ 有什么事%s？"):format(_.kana(npc))
            end,
            function(npc)
                return ("偶尔也想来顿奢侈的大餐%s。"):format(_.na(npc))
            end,
            function(npc)
                return ("没有神的存在，这个世界就毫无意义%s！"):format(_.da(npc))
            end,
            function(npc)
                return (
                    "以太之风似乎是由畸形之森引起的…呐，对此我不太感兴趣%s。"
                ):format(_.na(npc))
            end,
        },

        Ally = {
            "(一直盯着你看)",
            "…？",
        },

        Prostitute = {
            function(npc)
                return ("哎呀，你看起来不错%s哦。要不要让我陪你做个美梦？"):format(
                    _.loc("Elona.Gender.Names." .. _.gender(_.player()) .. ".Informal")
                )
            end,
        },

        Bored = {
            "(看着你，好像很无聊)",
            function(npc)
                return ("(%s瞥了你一眼，转过了头。)"):format(_.name(npc, true))
            end,
        },

        ChristmasFestival = {
            function(npc)
                return ("欢迎来到诺埃尔的圣夜庆典！玩得开心%s。"):format(_.kure(npc))
            end,
            function(npc)
                return (
                    "圣夜庆典，也被称为圣朱娅庆典%s。在这里，我们向治愈女神致敬，庆祝年末的盛宴%s。"
                ):format(_.ru(npc), _.noda(npc))
            end,
            function(npc)
                return (
                    "莫伊尔那家伙，今年的圣夜庆典上准备了一些特别的物品%s。"
                ):format(_.na(npc))
            end,
            function(npc)
                return (
                    "%s，你看见那座神圣的朱娅雕像了吗%s？真是美丽而娇媚%s！%s在这个时候改变信仰也是个好时机%s！"
                ):format(_.kimi(npc), _.kana(npc), _.ka(npc), _.kimi(npc), _.yo(npc))
            end,
            function(npc)
                return ("今晚和心爱的人做些愉快的事%s！"):format(_.noda(npc))
            end,
            function(npc)
                return (
                    "为了观看这个节日，许多游客在诺埃尔这个时候都会前来%s。"
                ):format(_.noda(npc))
            end,
            function(npc)
                return (
                    "圣夜庆典有着悠久的历史%s。传说中，朱娅的治愈之泪据说能够治愈任何重病%s。实际上，在庆典期间，曾目击到许多奇迹，比如盲人重见光明等%s。"
                ):format(_.da(npc), _.yo(npc))
            end,
        },

        Moyer = {
            "客人，不用害怕。这个怪物被魔法束缚，一动不动。来吧，顺便看看我们的商品。",
            "这就是那位统治了尼费亚迷宫贝隆的传说中的火巨人！今天来的你，实在是太幸运了！快来，欣赏这个既神圣又妖异的怪物吧。如果你购买了商品，甚至摸一下也没关系哦。",
        },

        Personality = {
            ["0"] = {
                function(npc)
                    return ("贾比国王是位聪明的人%s。"):format(_.da(npc))
                end,
                function(npc)
                    return ("克里默尔的酒想一直喝下去%s。"):format(_.na(npc))
                end,
                function(npc)
                    return ("猫为什么这么可爱%s？"):format(_.kana(npc))
                end,
                function(npc)
                    return ("耶尔斯最近崛起，成为了新的王国%s。"):format(_.da(npc))
                end,
                function(npc)
                    return (
                        "欧达纳比其他任何国家都更早地开始了尼费亚的探险和研究%s。"
                    ):format(_.noda(npc))
                end,
                function(npc)
                    return ("店主是不死之身%s。"):format(_.kana(npc))
                end,
                function(npc)
                    return ("似乎有方法可以知道神器的所在%s。"):format(
                        _.da(npc)
                    )
                end,
                function(npc)
                    return ("大胃的海豹喜欢吃鱼%s。"):format(_.da(npc))
                end,
                function(npc)
                    return (
                        "生病或身体不适时，充足的睡眠和休息是很重要的%s。而且传统上认为，祝福的恢复药水对疾病也有好处，从古时起就有人这么说%s。"
                    ):format(_.da(npc), _.ru(npc))
                end,
            },
            ["1"] = {
                function(npc)
                    return ("总之，金钱在世界上很重要%s。"):format(_.da(npc))
                end,
                function(npc)
                    return ("地上可能有掉落的钱%s。"):format(_.kana(npc))
                end,
                function(npc)
                    return ("对经济话题很感兴趣%s。"):format(_.noda(npc))
                end,
                function(npc)
                    return ("白金硬币不容易得到%s。"):format(_.na(npc))
                end,
                function(npc)
                    return ("欧达纳的财政似乎有些紧张%s。"):format(_.yo(npc))
                end,
                function(npc)
                    return (
                        "战后的赞恩虽然形式上是王国制度，但实际上是以艾斯·泰尔的诸国为模型的经济国家%s。"
                    ):format(_.da(npc))
                end,
            },
            ["2"] = {
                function(npc)
                    return (
                        "诺斯蒂利斯西部有许多遗迹和地下城，被称为尼费亚%s。"
                    ):format(_.noda(npc))
                end,
                function(npc)
                    return ("艾斯·泰尔是第七期文明%s。拥有先进的科技%s。"):format(
                        _.da(npc),
                        _.noda(npc)
                    )
                end,
                function(npc)
                    return (
                        "艾斯·泰尔似乎将魔法和科学视为对立的事物%s。"
                    ):format(_.da(npc))
                end,
                function(npc)
                    return ("喜欢谈论科学%s。"):format(_.da(npc))
                end,
                function(npc)
                    return ("维尔尼斯是帕尔米亚最大的矿业城%s。"):format(_.da(npc))
                end,
                function(npc)
                    return ("卢米埃斯特以艺术之城而闻名%s。"):format(_.dana(npc))
                end,
            },
            ["3"] = {
                function(npc)
                    return ("食物供应源最好提前确定%s。"):format(
                        _.yo(npc)
                    )
                end,
                function(npc)
                    return (
                        "尚未揭示的谜团在尼费亚中有很多，对于冒险者来说，这里就像一片圣地%s。"
                    ):format(_.da(npc))
                end,
                function(npc)
                    return ("永恒的誓约…？我从来没听说过这个词%s。"):format(_.na(npc))
                end,
                function(npc)
                    return ("谢拉·泰尔是第十一期文明%s。"):format(_.da(npc))
                end,
                function(npc)
                    return ("喜欢到处旅行%s。"):format(_.da(npc))
                end,
                function(npc)
                    return (
                        "如果冒险变得困难，可以尝试降低声望，也许会有帮助%s。"
                    ):format(_.na(npc))
                end,
                function(npc)
                    return ("宝箱是锁匠训练的好材料%s。"):format(_.da(npc))
                end,
            },
        },

        Rumor = {
            function(npc)
                return ("兔子的尾巴似乎能招来好运%s。"):format(_.dana(npc))
            end,
            function(npc)
                return (
                    "乞丐身上有一种净化身体的魔法护身符，他们可能会有%s。因为他们什么都吃%s。"
                ):format(_.da(npc), _.na(npc))
            end,
            function(npc)
                return ("僵尸偶尔会掉落奇怪的药水%s。"):format(_.da(npc))
            end,
            function(npc)
                return (
                    "说起来，以前听到过一个吟游诗人演奏奇异旋律的故事%s。我感动得扔掉了一双昂贵的鞋%s。"
                ):format(_.aru(npc), _.yo(npc))
            end,
            function(npc)
                return ("据说木乃伊偶尔会掉落可以复活死者的书%s。"):format(
                    _.da(npc)
                )
            end,
            function(npc)
                return ("看见了%s！行刑人被斩首，居然还能活过来！"):format(
                    _.noda(npc)
                )
            end,
            function(npc)
                return (
                    "被异形之眼盯上的人的身体会发生变化%s，有时甚至会掉落促使生物进化的药水%s。"
                ):format(_.ga(npc), _.yo(npc))
            end,
            function(npc)
                return ("据说精灵隐藏了非常神秘的经历%s！"):format(_.yo(npc))
            end,
            function(npc)
                return ("不要小看洛克斯罗的投石%s。"):format(_.yo(npc))
            end,
            function(npc)
                return (
                    "要小心银眼女巫%s。如果被她的大剑砍到，可没什么好下场%s。她有时候还带着一些奇怪的东西%s。"
                ):format(_.dana(npc), _.daro(npc), _.ga(npc))
            end,
            function(npc)
                return ("丘比特似乎在运送一些沉重的东西%s。"):format(_.yo(npc))
            end,
            function(npc)
                return ("梦中能见到神？"):format(_.kana(npc))
            end,
            function(npc)
                return (
                    "如果你遇到戴着黄色项圈、有四只手臂的怪物，最好赶紧逃走%s"
                ):format(_.dana(npc))
            end,
            "盗贼团的刺客偶尔会携带一种可以增加射击次数的魔法项圈。",
            function(npc)
                return ("贵族中有一些收集奇怪物品的人%s。"):format(
                    _.ga(npc)
                )
            end,
            function(npc)
                return (
                    "在聚会上演奏时，有时会有喝醉了的客人扔来一些奇怪的东西%s。"
                ):format(_.da(npc))
            end,
            function(npc)
                return ("看到过戴着红腰带的机器人%s。"):format(_.na(npc))
            end,
            function(npc)
                return (
                    "小恶魔持有的魔法卷轴可以改变神器的名字%s。"
                ):format(_.dana(npc))
            end,
            function(npc)
                return (
                    "约温的天真少女似乎珍惜着一些神秘的宝物%s。"
                ):format(_.da(npc))
            end,
            function(npc)
                return ("前些天看到了一个戴着非常漂亮的贝壳的土拨鼠%s。"):format(
                    _.yo(npc)
                )
            end,
            function(npc)
                return ("盗贼团的家伙们似乎经常使用一些可疑的药物%s。"):format(
                    _.na(npc)
                )
            end,
        },

        Shopkeeper = {
            function(npc)
                return ("经营店铺，实际上相当困难%s。"):format(_.na(npc))
            end,
            function(npc)
                return ("在其他店里，最好小心脚下，不要被人看不见%s。"):format(
                    _.yo(npc)
                )
            end,
            function(npc)
                return ("如果没有足够的力量赶走流氓，店主就不能安心经营%s。"):format(
                    _.yo(npc)
                )
            end,
            function(npc)
                return ("要为某个时刻选择继承店铺的人%s。"):format(_.ru(npc))
            end,
            function(npc)
                return ("欢迎光临。请慢慢看%s。"):format(_.kure(npc))
            end,
            function(npc)
                return ("我对商品的品种很有自信%s。"):format(_.noda(npc))
            end,
            function(npc)
                return ("好了，看看我的自豪商品%s。"):format(_.kure(npc))
            end,
            function(npc)
                return ("最近很多可疑人物，真是麻烦%s。"):format(_.da(npc))
            end,
            function(npc)
                return (
                    "武器和防具如果没有经过认证，就不能高价回购%s。"
                ):format(_.yo(npc))
            end,
        },

        Slavekeeper = {
            function(npc)
                return ("客人也是坏人%s。"):format(_.dana(npc))
            end,
            function(npc)
                return ("嘻嘻嘻…老板也是好色之徒%s。"):format(_.dana(npc))
            end,
        },
    },
}
