Elona.Tone = {
    ChangeTone = {
        Prompt = function(entity)
            return ("What sentence should %s learn? "):format(_.name(entity))
        end,
        IsSomewhatDifferent = function(entity)
            return ("%s is somewhat different."):format(_.name(entity))
        end,

        Layer = {
            Title = "Tone of Voice",
            Hint = {
                Action = {
                    ShowHidden = "Show Hidden",
                    ChangeTone = "Change Tone",
                },
            },
            DefaultTone = "Default Tone",
            ToneTitle = "Title",
            ModName = "Mod",
        },
    },
}
