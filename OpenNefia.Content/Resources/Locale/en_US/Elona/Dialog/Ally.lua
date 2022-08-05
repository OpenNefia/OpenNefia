Elona.Dialog.Ally = {
    Choices = {
        Abandon = "I'm going to abandon you.",
        AskForMarriage = "May I ask for your hand?",
        MakeGene = "Let's make a gene.",
        Silence = {
            Start = "Shut up.",
            Stop = "You can speak now.",
        },
        WaitAtTown = "Wait at the town.",
    },

    Abandon = {
        Choices = {
            Yes = "Yes.",
            No = "No.",
        },
        PromptConfirm = function(ally, player)
            return ("(%s looks at %s sadly. Really abandon %s?)"):format(_.name(ally), _.name(player), _.him(ally))
        end,
        YouAbandoned = function(ally, player)
            return ("%s abandoned %s..."):format(_.name(player), _.name(ally))
        end,
    },

    MakeGene = {
        Accepts = "*blush*",
        Refuses = "Not here!",
    },

    Marriage = {
        Accepts = "With pleasure.",
        Refuses = function(ally, player)
            return ("(%s gently refuses %s proposal. )"):format(_.name(ally), _.his(player))
        end,
    },

    Silence = {
        Start = function(ally)
            return ("(%s stops talking...)"):format(_.name(ally))
        end,
        Stop = function(ally, player)
            return ("(%s hugs %s.)"):format(_.name(ally), _.name(player))
        end,
    },

    WaitAtTown = function(ally, player)
        return ("(%s order%s %s to wait at the town.)"):format(_.name(player), _.s(player), _.name(ally))
    end,
}
