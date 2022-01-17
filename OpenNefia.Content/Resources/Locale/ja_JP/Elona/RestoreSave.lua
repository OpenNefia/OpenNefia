Elona.RestoreSave.Layer = {
    Window = {
        Title = "冒険者の選択",
    },

    Topic = {
        SaveName = "名前",
        SaveDate = "日付",
    },

    KeyHint = {
        Delete = "削除",
    },

    Caption = "どの冒険を再開するんだい？",

    Delete = {
        Confirm = function(saveName)
            return ("本当に%sを削除していいのかい？"):format(saveName)
        end,
        ConfirmFinal = function(saveName)
            return ("本当の本当に%sを削除していいのかい？"):format(saveName)
        end,
    },

    NoSaves = "セーブデータがありません",
}
