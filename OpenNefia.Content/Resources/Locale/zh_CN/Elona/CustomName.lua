Elona.CustomName = {
    Interact = {
        ChangeName = {
            Prompt = function(entity)
                return ("%s要叫什么名字呢？"):format(_.name(entity))
            end,
            YouNamed = function(entity, newName)
                return ("决定以%s这个名字称呼了。"):format(newName)
            end,
            Cancel = "放弃起名。",
        },
    },
}