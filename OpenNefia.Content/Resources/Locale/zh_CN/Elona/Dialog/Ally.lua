Elona.Dialog.Ally = {
    Choices = {
        Abandon = "切断缘分",
        AskForMarriage = "求婚",
        MakeGene = "留下遗传基因",
        Silence = {
            Start = "让其沉默",
            Stop = "让其开口",
        },
        WaitAtTown = "在城镇等待",
    },

    Abandon = {
        Choices = {
            Yes = "是",
            No = "否",
        },
        PromptConfirm = function(ally, player)
            return ("（%s用悲伤的眼神看着%s。确定要切断缘分吗？）"):format(
                _.name(ally),
                _.name(player)
            )
        end,
        YouAbandoned = function(ally, player)
            return ("%s%s分道扬镳…"):format(_.sore_wa(player), _.name(ally))
        end,
    },

    MakeGene = {
        Accepts = "哎呀，你这个人…",
        Refuses = "在这样的地方不行",
    },

    Marriage = {
        Accepts = "是...我很高兴。",
        Refuses = function(ally)
            return ("（%s委婉地拒绝了）"):format(_.name(ally))
        end,
    },

    Silence = {
        Start = function(ally)
            return ("（%s变得安静了…）"):format(_.name(ally))
        end,
        Stop = function(ally, player)
            return ("（%s拥抱了%s）"):format(_.name(ally), _.name(player))
        end,
    },

    WaitAtTown = function(ally, player)
        return ("（%s指示%s在城镇等待）"):format(_.name(player), _.name(ally))
    end,
}