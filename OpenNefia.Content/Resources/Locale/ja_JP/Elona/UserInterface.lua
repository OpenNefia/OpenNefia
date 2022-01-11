Elona.UserInterface = {
    Common = {
        Yes = "ああ",
        No = "いや…",
    },
    Save = {
        QuickSave = " *保存* ",
    },
    Exit = {
        Prompt = {
            Text = "これまでの冒険を記録して終了する？",
            Choices = {
                Cancel = "いいえ",
                Exit = "はい",
                GameSetting = "ゲーム設定",
                ReturnToTitle = "タイトルに戻る",
            },
        },

        Saved = "無事に記録された。",
        YouCloseYourEyes = function(player)
            return ("%sは静かに目を閉じた… (キーを押すと自動終了します)"):format(_.name(player))
        end,
    },
}
