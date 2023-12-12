Elona.Encounter = {
    Types = {
        Enemy = {
            Message = function(distNearestTown, rank)
                return ("襲撃だ！ (最も近い街までの距離:%s 敵勢力:%s)"):format(distNearestTown, rank)
            end,
            Rank = {
                Putit = "プチ級",
                Orc = "オーク級",
                GrizzlyBear = "グリズリー級",
                Drake = "ドレイク級",
                Lich = "リッチ級",
                Dragon = "ドラゴン級",
            },
        },
    },
}
