Elona.UserInterface = {
    Common = {
        Yes = "是",
        No = "否",
    },
    Save = {
        QuickSave = " *保存* ",
    },
    Exit = {
        Prompt = {
            Text = "是否保存并退出现在的冒险？",
            Choices = {
                Cancel = "取消",
                GameSetting = "游戏设置",
                ReturnToTitle = "返回标题",
                Exit = "退出",
            },
            PromptSaveGame = "是否保存游戏？",
        },

        Saved = "成功保存。",
        YouCloseYourEyes = function(player)
            return ("%s静静地闭上了眼睛... (按键自动退出)"):format(_.name(player))
        end,
    },
}