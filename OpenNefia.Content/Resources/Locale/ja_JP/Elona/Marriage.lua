Elona.Marriage = {
    Event = {
        Title = "結婚",
        Text = function(source, target)
            return (
                "長い交際の末、遂に%sと%sは固い絆で結ばれた。婚儀の後、%sの元に幾つか祝儀品が届けられた。"
            ):format(_.name(source, true), _.name(target, true), _.name(source, true))
        end,
        Choices = {
            ["0"] = "生涯をあなたに捧げる",
        },
    },
}
