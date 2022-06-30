Elona.UserInterface = {
    Common = {
        Yes = "Yes",
        No = "No..",
    },
    Save = {
        QuickSave = " *Save* ",
    },
    Exit = {
        Prompt = {
            Text = "Do you want to save the game and exit?",
            Choices = {
                Cancel = "Cancel",
                GameSetting = "Game Setting",
                ReturnToTitle = "Return to Title",
                Exit = "Exit",
            },
            PromptSaveGame = "Save the game?",
        },

        Saved = "Your game has been saved successfully.",
        YouCloseYourEyes = function(entity)
            return ("%s close%s %s eyes and peacefully fade%s away. (Hit any key to exit)"):format(
                _.name(entity),
                _.s(entity),
                _.his(entity),
                _.s(entity)
            )
        end,
    },
}
