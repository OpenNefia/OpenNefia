Elona.CustomName = {
    Interact = {
        ChangeName = {
            Prompt = function(entity)
                return ("%sを何と呼ぶ？ "):format(_.name(entity))
            end,
            YouNamed = function(entity, newName)
                return ("%sという名前で呼ぶことにした。"):format(newName)
            end,
            Cancel = "名前をつけるのはやめた。",
        },
    },
}
