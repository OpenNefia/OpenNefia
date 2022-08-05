Elona.Dialog.Ally = {
    Choices = {
        Abandon = "縁を切る",
        AskForMarriage = "婚約を申し込む",
        MakeGene = "遺伝子を残す",
        Silence = {
            Start = "黙らせる",
            Stop = "喋らせる",
        },
        WaitAtTown = "街で待機しろ",
    },

    Abandon = {
        Choices = {
            Yes = "切る",
            No = "やめる",
        },
        PromptConfirm = function(ally, player)
            return ("(%sは悲しそうな目で%sを見ている。本当に縁を切る？)"):format(
                _.name(ally),
                _.name(player)
            )
        end,
        YouAbandoned = function(ally, player)
            return ("%s%sと別れた…"):format(_.sore_wa(player), _.name(ally))
        end,
    },

    MakeGene = {
        Accepts = "いやん、あなたったら…",
        Refuses = "こんな場所では嫌よ",
    },

    Marriage = {
        Accepts = "はい…喜んで。",
        Refuses = function(ally)
            return ("(%sはやんわりと断った)"):format(_.name(ally))
        end,
    },

    Silence = {
        Start = function(ally)
            return ("(%sはしゅんとなった…)"):format(_.name(ally))
        end,
        Stop = function(ally, player)
            return ("(%sは%sに抱きついた)"):format(_.name(ally), _.name(player))
        end,
    },

    WaitAtTown = function(ally, player)
        return ("(%sは、%sに街で待っているように指示した)"):format(_.name(player), _.name(ally))
    end,
}
