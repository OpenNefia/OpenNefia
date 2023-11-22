Elona.Stayers = {
    Manage = {
        Window = {
            Title = "滞在状態の変更",
        },
        Prompt = "誰を滞在させる？",

        Add = {
            Ally = function(entity)
                return ("%sを滞在させた。"):format(_.basename(entity))
            end,
            Worker = function(entity)
                return ("%sを任命した。"):format(_.basename(entity))
            end,
        },
        Remove = {
            Ally = function(entity)
                return ("%sの滞在を取り消した。"):format(_.basename(entity))
            end,
            Worker = function(entity)
                return ("%sを役目から外した。"):format(_.basename(entity))
            end,
        },
    },
}
