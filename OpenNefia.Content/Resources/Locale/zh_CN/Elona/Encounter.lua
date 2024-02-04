Elona.Encounter = {
    Types = {
        Enemy = {
            Message = function(distNearestTown, rank)
                return ("袭击！（距离最近的城镇：%s 敌方势力：%s）"):format(distNearestTown, rank)
            end,
            Rank = {
                Putit = "小精灵级",
                Orc = "兽人级",
                GrizzlyBear = "灰熊级",
                Drake = "龙兽级",
                Lich = "巫妖级",
                Dragon = "巨龙级",
            },
        },
    },
}