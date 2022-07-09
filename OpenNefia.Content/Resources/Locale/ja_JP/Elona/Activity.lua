Elona.Activity = {
    DefaultVerb = "行動",
    Cancel = {
        Normal = function(actor, activity)
            return ("%sは%sを中断した。"):format(_.name(actor), _.name(activity))
        end,
        Item = function(actor)
            return ("%sは行動を中断した。"):format(_.name(actor))
        end,
        Prompt = function(activity)
            return ("%sを中断したほうがいいだろうか？ "):format(_.name(activity))
        end,
    },

    Resting = {
        Start = "あなたは横になった。",
        Finish = "あなたは休息を終えた。",
        DropOffToSleep = "あなたはそのまま眠りにおちた…",
    },
}
