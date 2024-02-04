Elona.Activity = {
    DefaultVerb = "行动",
    Cancel = {
        Normal = function(actor, activity)
            return ("%s中断了%s。"):format(_.name(actor), _.name(activity))
        end,
        Item = function(actor)
            return ("%s中断了行动。"):format(_.name(actor))
        end,
        Prompt = function(activity)
            return ("是否中断%s？"):format(_.name(activity))
        end,
    },

    Resting = {
        Start = "你躺下了。",
        Finish = "你结束了休息。",
        DropOffToSleep = "你不知不觉睡着了...",
    },

    Eating = {
        Start = {
            Normal = function(actor, food)
                return ("%s把%s送入口中。"):format(_.name(actor), _.name(food, nil, 1))
            end,
            InSecret = function(actor, food)
                return ("%s悄悄地把%s送入口中。"):format(_.name(actor), _.name(food, nil, 1))
            end,
            Mammoth = _.quote "开动吧，猛犸象",
        },
        Finish = function(actor, food)
            return ("%s吃完了%s。"):format(_.sore_wa(actor), _.name(food, nil, 1))
        end,
    },

    Sex = {
        TakesClothesOff = function(actor)
            return ("%s开始脱衣服。"):format(_.name(actor))
        end,
        Dialog = {
            "“唔”",
            "“嗯嗯”",
            "“好气哦，不过...”",
            "“哈哈！”",
            "“呵呵呵”",
        },
        DialogAfter = {
            function(partner)
                return ("太好了%s"):format(_.yo(partner, 3))
            end,
            function(partner)
                return ("太, 太厉害了%s"):format(_.yo(partner, 3))
            end,
            function(partner)
                return ("不, 不行了%s"):format(_.da(partner, 3))
            end,
            function(partner)
                return ("太, 太激烈了%s"):format(_.yo(partner, 3))
            end,
            function(partner)
                return ("哇, 完全失败了%s"):format(_.da(partner, 3))
            end,
        },
        GetsFurious = function(actor)
            return ("%s发怒了。“你在侮辱我吗？”"):format(_.name(actor))
        end,
        SpareLife = function(actor, partner)
            return ("“我, 我们之间只是身体的关系%s%s一无所知，请只是保留我的命...！”"):format(
                _.loc("Elona.Gender.Names." .. _.gender(actor) .. ".Informal"),
                _.da(partner),
                _.ore(partner, 3)
            )
        end,
        Take = function(partner)
            return ("来吧，接收一下零用钱%s"):format(_.kure(partner, 3))
        end,
        TakeAllIHave = function(partner)
            return ("这是%s的钱包里所有的东西%s"):format(_.ore(partner, 3), _.da(partner))
        end,
    },

    Performing = {
        Dialog = {
            Angry = {
                _.quote "退下！",
                _.quote "吵死了！",
                _.quote "弄砸了！",
                _.quote "你打算干嘛！",
            },
            Disinterest = { _.quote "厌了", _.quote "之前听过了", _.quote "又是这首曲子..." },
            Interest = {
                function(audience)
                    return ("%s欢呼起来。"):format(_.name(audience))
                end,
                function(audience)
                    return ("%s眼睛发亮。"):format(_.name(audience))
                end,
                _.quote "太棒了",
                _.quote "不错哦！",
                function(audience)
                    return ("%s陶醉其中。"):format(_.name(audience))
                end,
                function(audience, actor)
                    return ("%s赞扬了%s的表演。"):format(_.name(audience), _.name(actor))
                end,
            },
        },
        GetsAngry = function(audience)
            return ("%s生气了。"):format(_.name(audience))
        end,
        Quality = {
            ["0"] = "完全失败...",
            ["1"] = "不受欢迎...",
            ["2"] = "还需要更多练习...",
            ["3"] = "演奏结束了。",
            ["4"] = "不怎么样。",
            ["5"] = "进展不错。",
            ["6"] = "成功的演奏。",
            ["7"] = "大家热烈欢迎！",
            ["8"] = "太棒了！",
            ["9"] = "名演奏将载入史册！",
        },
        Sound = {
            Cha = "ﾁｬﾝ♪ ",
            Random = { "ﾁｬﾗﾝ♪ ", "ﾎﾟﾛﾝ♪ ", "ﾀﾞｰﾝ♪ " },
        },
        Start = function(actor, instrument)
            return ("%s开始演奏%s。"):format(_.name(actor), _.name(instrument))
        end,
        ThrowsRock = function(audience)
            return ("%s扔了块石头。"):format(_.name(audience))
        end,
        Tip = function(actor, tips)
            return ("%s共收到了%s的小费。"):format(_.name(actor), tips)
        end,
    },
}