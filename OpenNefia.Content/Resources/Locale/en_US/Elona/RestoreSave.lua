Elona.RestoreSave.Layer = {
    Window = {
        Title = "Game Selection",
    },

    Topic = {
        SaveName = "Name",
        SaveDate = "Date",
    },

    KeyHint = {
        Delete = "Delete",
    },

    Caption = "Which save game do you want to continue?",

    Delete = {
        Confirm = function(saveName)
            return ("Do you really want to delete %s ?"):format(saveName)
        end,
        ConfirmFinal = function(saveName)
            return ("Are you sure you really want to delete %s ?"):format(saveName)
        end,
    },

    NoSaves = "No save files found",
}
