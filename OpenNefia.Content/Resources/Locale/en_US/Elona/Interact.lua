Elona.Interact = {
    Query = {
        Direction = "Choose the direction of the target.",
        Action = function(target)
            return ("What action do you want to perform to %s?"):format(_.him(target))
        end,
    },
    NoInteractActions = function(target)
        return ("There's no way to interact with %s."):format(_.him(target))
    end,

    Actions = {
        Appearance = "Appearance",
        Attack = "Attack",
        BringOut = "Bring Out",
        ChangeTone = "Change Tone",
        Equipment = "Equipment",
        Give = "Give",
        Info = "Info",
        Inventory = "Inventory",
        Items = "Items",
        Name = "Name",
        Release = "Release",
        Talk = "Talk",
        TeachWords = "Teach Words",
    },
}
