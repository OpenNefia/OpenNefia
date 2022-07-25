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
}
