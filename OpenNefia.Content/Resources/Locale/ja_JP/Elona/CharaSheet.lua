Elona.CharaSheet = {
    Topic = {
        Attribute = "能力(元の値)  - 潜在能力",
        Blessing = "祝福と呪い",
        Trace = "冒険の軌跡",
        Extra = "その他",
        Rolls = "各種修正",
    },

    KeyHint = {
        BlessingAndHex = "祝福と呪いの情報",
        SpendBonus = "ボーナスの分配",
        TrackSkill = "スキルトラック",
        TrainSkill = "スキルを訓練",
        LearnSkill = "スキルを習得",
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
            Fame = "名声",
            Karma = "カルマ",
            Sanity = "狂気度",
        },

        Exp = {
            Exp = "経験",
            God = "信仰",
            Guild = "所属",
            Level = "レベル",
            RequiredExp = "必要値",
        },

        BlessingHex = {
            HintTopic = "説明:",
            HintBody = function(buffName, buffDesc, turns)
                return ("%s: (%s)ﾀｰﾝの間、%s"):format(buffName, turns, buffDesc)
            end,
            NotAffected = "今は持続効果を受けていない",
        },

        Personal = {
            Name = "名前",
            Alias = "異名",
            Race = "種族",
            Sex = "性別",
            Class = "職業",
            Age = "年齢",
            AgeCounter = function(years)
                return ("%s 歳"):format(years)
            end,
            Height = "身長",
            Cm = "cm",
            Weight = "体重",
            Kg = "kg",
        },

        Trace = {
            Turns = "ターン",
            TurnsCounter = function(turns)
                return ("%sターン"):format(turns)
            end,
            Days = "経過日",
            DaysCounter = function(days)
                return ("%s日"):format(days)
            end,
            Kills = "殺害数",
            Time = "総時間",
        },

        Extra = {
            CargoWeight = "荷車重量",
            CargoLimit = "荷車限界",
            EquipWeight = "装備重量",
            DeepestLevel = "最深到達",
            DeepestLevelCounter = function(level)
                return ("%s階相当"):format(level)
            end,
        },
    },
}
