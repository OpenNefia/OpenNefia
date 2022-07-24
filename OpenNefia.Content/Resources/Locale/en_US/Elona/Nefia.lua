Elona.Nefia = {
    NoDungeonMaster = "This place is pretty dull. The dungeon master is no longer sighted here.",

    Level = function(floorNumber)
        return ("%s"):format(_.ordinal(floorNumber))
    end,

    Event = {
        ReachedDeepestLevel = "It seems you have reached the deepest level of this dungeon.",
        GuardedByLord = function(mapEntity, bossEntity)
            return ("Be aware! This level is guarded by the lord of %s, %s."):format(mapEntity, _.basename(bossEntity))
        end,
    },

    NameModifiers = {
        TypeA = {
            Rank0 = function(baseName)
                return ("Beginner's %s"):format(baseName)
            end,
            Rank1 = function(baseName)
                return ("Adventurer's %s"):format(baseName)
            end,
            Rank2 = function(baseName)
                return ("Dangerous %s"):format(baseName)
            end,
            Rank3 = function(baseName)
                return ("Fearful %s"):format(baseName)
            end,
            Rank4 = function(baseName)
                return ("King's %s"):format(baseName)
            end,
        },
        TypeB = {
            Rank0 = function(baseName)
                return ("Safe %s"):format(baseName)
            end,
            Rank1 = function(baseName)
                return ("Exciting %s"):format(baseName)
            end,
            Rank2 = function(baseName)
                return ("Servant's %s"):format(baseName)
            end,
            Rank3 = function(baseName)
                return ("Shadow %s"):format(baseName)
            end,
            Rank4 = function(baseName)
                return ("Chaotic %s"):format(baseName)
            end,
        },
    },
}
