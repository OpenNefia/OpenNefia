Elona.Dialog.Guard = {
    Choices = {
        WhereIs = function(entity)
            return ("%sの居場所を聞く"):format(_.name(entity, true))
        end,
        LostProperty = function(item)
            return ("落し物の%sを届ける"):format(_.basename(item))
        end,
    },

    LostProperty = {
        TurnIn = {
            Dialog = function(speaker)
                return ("わざわざ落し物を届けてくれた%s%sは市民の模範%s%s"):format(
                    _.noka(speaker),
                    _.kimi(speaker, 3),
                    _.da(speaker),
                    _.thanks(speaker)
                )
            end,
            Choice = "当然のことだ",
        },
        Empty = {
            Dialog = function(speaker)
                return ("む…中身が空っぽ%s"):format(_.dana(speaker, 2))
            end,
            Choice = "しまった…",
        },
        FoundOften = {
            Dialog = {
                function(speaker)
                    return ("む、また%s%s随分と頻繁に財布を見つけられるもの%s"):format(
                        _.kimi(speaker, 3),
                        _.ka(speaker),
                        _.dana(speaker)
                    )
                end,
                "（…あやしい）",
            },
            Choice = "ぎくっ",
        },
    },

    WhereIs = {
        Dead = function(speaker)
            return ("奴なら今は死んでいる%s"):format(_.yo(speaker, 2))
        end,
        VeryClose = function(speaker, target, direction)
            return ("%sならすぐ近くにいる%s%sの方を向いてごらん。"):format(
                _.name(target, true),
                _.yo(speaker),
                direction
            )
        end,
        Close = function(speaker, target, direction)
            return ("ちょっと前に%sの方で見かけた%s"):format(direction, _.yo(speaker))
        end,
        Moderate = function(speaker, target, direction)
            return ("%sなら%sの方角を探してごらん。"):format(_.name(target, true), direction)
        end,
        Far = function(speaker, target, direction)
            return ("%sに会いたいのなら、%sにかなり歩く必要があ%s"):format(
                _.name(target, true),
                direction,
                _.ru(speaker)
            )
        end,
        VeryFar = function(speaker, target, direction)
            return ("%s%s、ここから%sの物凄く離れた場所にいるはず%s"):format(
                _.name(target, true),
                _.ka(speaker, 3),
                direction,
                _.da(speaker)
            )
        end,
    },
}
