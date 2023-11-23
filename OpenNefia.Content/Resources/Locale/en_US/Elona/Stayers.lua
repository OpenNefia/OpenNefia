Elona.Stayers = {
    Manage = {
        Window = {
            Title = "Ally List",
        },
        Prompt = "Who stays in your home?",

        Add = {
            Ally = function(entity)
                return ("%s stay%s at your home now."):format(_.basename(entity), _.s(entity))
            end,
            Worker = function(entity)
                return ("%s take%s charge of the job now."):format(_.basename(entity), _.s(entity))
            end,
        },
        Remove = {
            Ally = function(entity)
                return ("%s %s no longer staying at your home."):format(_.basename(entity), _.is(entity))
            end,
            Worker = function(entity)
                return ("You remove %s from %s job."):format(_.basename(entity), _.his(entity))
            end,
        },
    },
}
