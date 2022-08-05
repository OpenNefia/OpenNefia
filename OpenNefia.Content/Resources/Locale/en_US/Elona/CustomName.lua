Elona.CustomName = {
    Interact = {
        ChangeName = {
            Prompt = function(entity)
                return ("What do you want to call %s? "):format(_.him(entity))
            end,
            YouNamed = function(entity, newName)
                return ("You named %s %s."):format(_.him(entity), newName)
            end,
            Cancel = "You changed your mind.",
        },
    },
}
