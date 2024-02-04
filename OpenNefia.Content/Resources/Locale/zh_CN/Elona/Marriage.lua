Elona.Marriage = {
    Event = {
        Title = "结婚",
        Text = function(source, target)
            return (
                "经过长时间的交往，%s和%s终于以坚固的纽带结为一体。婚礼之后，%s收到了一些祝贺礼物。"
            ):format(_.name(source, true), _.name(target, true), _.name(source, true))
        end,
        Choices = {
            ["0"] = "将一生奉献给你",
        },
    },
}