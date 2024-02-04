Elona.Stayers = {
    Manage = {
        Window = {
            Title = "滞在状态变更",
        },
        Prompt = "谁要滞留？",

        Add = {
            Ally = function(entity)
                return ("让%s滞留。"):format(_.basename(entity))
            end,
            Worker = function(entity)
                return ("任命%s。"):format(_.basename(entity))
            end,
        },
        Remove = {
            Ally = function(entity)
                return ("取消%s的滞留。"):format(_.basename(entity))
            end,
            Worker = function(entity)
                return ("剥夺%s的职责。"):format(_.basename(entity))
            end,
        },
    },
}