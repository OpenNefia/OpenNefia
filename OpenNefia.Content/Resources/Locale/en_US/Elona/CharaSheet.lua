Elona.CharaSheet = {
    Topic = {
        Attribute = "Attributes(Org) - Potential",
        Blessing = "Blessing and Hex",
        Trace = "Trace",
        Extra = "Extra Info",
        Rolls = "Combat Rolls",
    },

    KeyHint = {
        BlessingAndHex = "Blessing/Curse Info",
        SpendBonus = "Spend Bonus",
        TrackSkill = "Track Skill",
        TrainSkill = "Train Skill",
        LearnSkill = "Learn Skill",
    },

    Potential = {
        Superb = "Superb",
        Great = "Great",
        Good = "Good",
        Bad = "Bad",
        Hopeless = "Hopeless",
    },

    Group = {
        Attribute = {
            Fame = "Fame",
            Karma = "Karma",
            Sanity = "Sanity",
        },

        Exp = {
            Exp = "EXP",
            God = "God",
            Guild = "Guild",
            Level = "Level",
            RequiredExp = "Next Lv",
        },

        BlessingHex = {
            HintTopic = "Hint:",
            HintBody = function(buffName, buffDesc, turns)
                return ("%s: (%s) %s"):format(buffName, turns, buffDesc)
            end,
            NotAffected = "This character isn't currently blessed or hexed.",
        },

        Personal = {
            Name = "Name",
            Alias = "Aka",
            Race = "Race",
            Sex = "Sex",
            Class = "Class",
            Age = "Age",
            AgeCounter = function(years)
                return ("%s"):format(years)
            end,
            Height = "Height",
            Cm = "cm",
            Weight = "Weight",
            Kg = "kg",
        },

        Trace = {
            Turns = "Turns",
            TurnsCounter = function(turns)
                return ("%s Turns"):format(turns)
            end,
            Days = "Days",
            DaysCounter = function(days)
                return ("%s Days"):format(days)
            end,
            Kills = "Kills",
            Time = "Time",
        },

        Extra = {
            CargoWeight = "Cargo Wt",
            CargoLimit = "Cargo Lmt",
            EquipWeight = "Equip Wt",
            DeepestLevel = "Deepest Lv",
            DeepestLevelCounter = function(level)
                return _.ordinal(level) .. " Level"
            end,
        },
    },
}
