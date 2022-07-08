Elona.Door = {
    QueryClose = "何を閉める？",
    Open = {
        Succeeds = function(entity)
            return ("%sは扉を開けた。"):format(_.name(entity))
        end,
        Fails = function(entity)
            return ("%s開錠に失敗した。"):format(_.sore_wa(entity))
        end,
    },
    Close = {
        Succeeds = function(entity)
            return ("%sは扉を閉めた。"):format(_.name(entity))
        end,
        Blocked = "何かが邪魔で閉められない。",
        NothingToClose = "その方向に閉められるものはない。",
    },
}
