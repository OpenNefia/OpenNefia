Elona.RestoreSave.Layer = {
    Window = {
        Title = "冒険者选择",
    },

    Topic = {
        SaveName = "名称",
        SaveDate = "日期",
    },

    KeyHint = {
        Delete = "删除",
    },

    Caption = "要继续哪次冒险呢？",

    Delete = {
        Confirm = function(saveName)
            return ("确定要删除%s吗？"):format(saveName)
        end,
        ConfirmFinal = function(saveName)
            return ("确定要彻底删除%s吗？"):format(saveName)
        end,
    },

    NoSaves = "没有存档数据",
}