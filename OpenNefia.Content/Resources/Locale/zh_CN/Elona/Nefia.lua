Elona.Nefia = {
    BossMayDisappear = "如果现在离开地下城，就无法完成这一层的任务...",
    NoDungeonMaster = function(mapEntity)
        return ("周围没有任何紧张感了。%s的主人似乎已经不在了。"):format(
            _.name(mapEntity)
        )
    end,
    PromptGiveUpQuest = "放弃任务并前往下一层？",

    Level = function(floorNumber)
        return ("%s层"):format(_.ordinal(floorNumber))
    end,
    EntranceMessage = function(area, level)
        return ("%s有入口（入口难度相当于%s层）。"):format(_.name(area, true), level)
    end,

    Event = {
        ReachedDeepestLevel = "看起来到达了最深层...",
        GuardedByLord = function(mapEntity, bossEntity)
            return ("小心！这一层被%s的守护者%s保护着。"):format(
                _.name(mapEntity),
                _.basename(bossEntity)
            )
        end,
    },

    NameModifiers = {
        TypeA = {
            Rank0 = function(baseName)
                return ("起源的%s"):format(baseName)
            end,
            Rank1 = function(baseName)
                return ("冒险者的%s"):format(baseName)
            end,
            Rank2 = function(baseName)
                return ("迷失的%s"):format(baseName)
            end,
            Rank3 = function(baseName)
                return ("死亡的%s"):format(baseName)
            end,
            Rank4 = function(baseName)
                return ("无归的%s"):format(baseName)
            end,
        },
        TypeB = {
            Rank0 = function(baseName)
                return ("安全的%s"):format(baseName)
            end,
            Rank1 = function(baseName)
                return ("激动人心的%s"):format(baseName)
            end,
            Rank2 = function(baseName)
                return ("勇者的%s"):format(baseName)
            end,
            Rank3 = function(baseName)
                return ("黑暗的%s"):format(baseName)
            end,
            Rank4 = function(baseName)
                return ("混沌的%s"):format(baseName)
            end,
        },
    },
}