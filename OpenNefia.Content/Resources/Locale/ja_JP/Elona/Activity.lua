Elona.Activity = {
    DefaultVerb = "行動",
    Cancel = {
        Normal = function(actor, activity)
            return ("%sは%sを中断した。"):format(_.name(actor), _.name(activity))
        end,
        Item = function(actor)
            return ("%sは行動を中断した。"):format(_.name(actor))
        end,
        Prompt = function(activity)
            return ("%sを中断したほうがいいだろうか？ "):format(_.name(activity))
        end,
    },

    Resting = {
        Start = "あなたは横になった。",
        Finish = "あなたは休息を終えた。",
        DropOffToSleep = "あなたはそのまま眠りにおちた…",
    },

    Eating = {
        Start = {
            Normal = function(actor, food)
                return ("%sは%sを口に運んだ。"):format(_.name(actor), _.name(food, nil, 1))
            end,
            InSecret = function(actor, food)
                return ("%sは%sをこっそりと口に運んだ。"):format(_.name(actor), _.name(food, nil, 1))
            end,
            Mammoth = _.quote "いただきマンモス",
        },
        Finish = function(actor, food)
            return ("%s%sを食べ終えた。"):format(_.sore_wa(actor), _.name(food, nil, 1))
        end,
    },

    Sex = {
        Gender = {
            Male = "男",
            Female = "女",
        },
        TakesClothesOff = function(actor)
            return ("%sは服を脱ぎ始めた。"):format(_.name(actor))
        end,
        Dialog = {
            "「きくぅ」",
            "「もふもふ」",
            "「くやしい、でも…」",
            "「はぁはぁ！」",
            "「ウフフフ」",
        },
        DialogAfter = {
            function(partner)
                return ("よかった%s"):format(_.yo(partner, 3))
            end,
            function(partner)
                return ("す、すごい%s"):format(_.yo(partner, 3))
            end,
            function(partner)
                return ("も、もうだめ%s"):format(_.da(partner, 3))
            end,
            function(partner)
                return ("は、激しかった%s"):format(_.yo(partner, 3))
            end,
            function(partner)
                return ("か、完敗%s"):format(_.da(partner, 3))
            end,
        },
        GetsFurious = function(actor)
            return ("%sは激怒した。「なめてんの？」"):format(_.name(actor))
        end,
        SpareLife = function(actor, partner)
            return ("「そ、その%sとは体だけの関係%s%sは何も知らないから、命だけは…！」"):format(
                _.loc("Elona.Activity.Sex.Gender." .. _.gender(actor)),
                _.da(partner),
                _.ore(partner, 3)
            )
        end,
        Take = function(partner)
            return ("さあ、小遣いを受け取って%s"):format(_.kure(partner, 3))
        end,
        TakeAllIHave = function(partner)
            return ("これが%sの財布の中身の全て%s"):format(_.ore(partner, 3), _.da(partner))
        end,
    },

    Performing = {
        Dialog = {
            Angry = {
                _.quote "引っ込め！",
                _.quote "うるさい！",
                _.quote "下手っぴ！",
                _.quote "何のつもりだ！",
            },
            Disinterest = { _.quote "飽きた", _.quote "前にも聴いたよ", _.quote "またこの曲か…" },
            Interest = {
                function(audience)
                    return ("%sは歓声を上げた。"):format(_.name(audience))
                end,
                function(audience)
                    return ("%sは目を輝かせた。"):format(_.name(audience))
                end,
                _.quote "ブラボー",
                _.quote "いいぞ！",
                function(audience)
                    return ("%sはうっとりした。"):format(_.name(audience))
                end,
                function(audience, actor)
                    return ("%sは%sの演奏を褒め称えた。"):format(_.name(audience), _.name(actor))
                end,
            },
        },
        GetsAngry = function(audience)
            return ("%sは激怒した。"):format(_.name(audience))
        end,
        Quality = {
            ["0"] = "まるで駄目だ…",
            ["1"] = "不評だった…",
            ["2"] = "もっと練習しなくては…",
            ["3"] = "演奏を終えた。",
            ["4"] = "いまいちだ。",
            ["5"] = "手ごたえがあった。",
            ["6"] = "納得できる演奏ができた。",
            ["7"] = "大盛況だ！",
            ["8"] = "素晴らしい！",
            ["9"] = "歴史に残る名演だ！",
        },
        Sound = {
            Cha = "ｼﾞｬﾝ♪ ",
            Random = { "ﾁｬﾗﾝ♪ ", "ﾎﾟﾛﾝ♪ ", "ﾀﾞｰﾝ♪ " },
        },
        Start = function(actor, instrument)
            return ("%sは%sの演奏をはじめた。"):format(_.name(actor), _.name(instrument))
        end,
        ThrowsRock = function(audience)
            return ("%sは石を投げた。"):format(_.name(audience))
        end,
        Tip = function(actor, tips)
            return ("%sは合計 %sのおひねりを貰った。"):format(_.name(actor), tips)
        end,
    },
}
