Elona.Dialog.Unique.RogueBoss = {
    TooPoor = {
        Text = "Bah, a beggar without a penny. What a waste of time! Just go away!",
    },

    Ambush = {
        Text = function(speaker, rogueGroupName, surrenderCost)
            return (
                "Halt, halt, traveler. You're a quite fortunate one. Before you is the renowned band of legendary brigands \"%s\" that the mere mention of its name is enough to silence a naughty child. Yet we will spare your life for only a toll of %s gold pieces and your cargos. Quite fortunate indeed."
            ):format(rogueGroupName, surrenderCost)
        end,
        Choices = {
            Surrender = "I surrender.",
            TryMe = "Try me.",
        },
    },

    TryMe = {
        Text = "You've got some guts. But your decision sure ain't a wise one. This will be your grave, kid.",
    },

    Surrender = {
        Text = "A wise choice.",
    },
}
