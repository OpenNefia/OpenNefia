Elona.Nefia = {
    NoDungeonMaster = function(mapEntity)
        return ("辺りからは何の緊張感も感じられない。%sの主はもういないようだ。"):format(
            _.name(mapEntity)
        )
    end,
    Level = function(floorNumber)
        return ("%s層"):format(_.ordinal(floorNumber))
    end,

    Event = {
        ReachedDeepestLevel = "どうやら最深層まで辿り着いたらしい…",
        GuardedByLord = function(mapEntity, bossEntity)
            return ("気をつけろ！この階は%sの守護者、%sによって守られている。"):format(
                mapEntity,
                _.basename(bossEntity)
            )
        end,
    },

    Names = {
        TypeA = {
            Rank0 = function(baseName)
                return ("はじまりの%s"):format(baseName)
            end,
            Rank1 = function(baseName)
                return ("冒険者の%s"):format(baseName)
            end,
            Rank2 = function(baseName)
                return ("迷いの%s"):format(baseName)
            end,
            Rank3 = function(baseName)
                return ("死の%s"):format(baseName)
            end,
            Rank4 = function(baseName)
                return ("不帰の%s"):format(baseName)
            end,
        },
        TypeB = {
            Rank0 = function(baseName)
                return ("安全な%s"):format(baseName)
            end,
            Rank1 = function(baseName)
                return ("時めきの%s"):format(baseName)
            end,
            Rank2 = function(baseName)
                return ("勇者の%s"):format(baseName)
            end,
            Rank3 = function(baseName)
                return ("闇の%s"):format(baseName)
            end,
            Rank4 = function(baseName)
                return ("混沌の%s"):format(baseName)
            end,
        },
    },
}
