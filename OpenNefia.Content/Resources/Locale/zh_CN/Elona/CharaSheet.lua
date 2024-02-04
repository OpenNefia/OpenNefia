Elona.CharaSheet = {
    Topic = {
        Attribute = "能力(原始值) - 潜在能力",
        Blessing = "祝福和诅咒",
        Trace = "冒险轨迹",
        Extra = "其他",
        Rolls = "各种修正",
    },

    KeyHint = {
        BlessingAndHex = "祝福和诅咒信息",
        SpendBonus = "分配奖励",
        TrackSkill = "追踪技能",
        TrainSkill = "训练技能",
        LearnSkill = "学习技能",
    },

    Potential = {
        Superb = "极好",
        Great = "很好",
        Good = "好",
        Bad = "差",
        Hopeless = "绝望",
    },

    Group = {
        Attribute = {
            Fame = "声望",
            Karma = "业力",
            Sanity = "疯狂度",
        },

        Exp = {
            Exp = "经验值",
            God = "信仰",
            Guild = "所属",
            Level = "等级",
            RequiredExp = "所需经验",
        },

        BlessingHex = {
            HintTopic = "说明:",
            HintBody = function(buffName, buffDesc, turns)
                return ("%s: (%s)回合内 %s"):format(buffName, turns, buffDesc)
            end,
            NotAffected = "目前未受持续效果影响",
        },

        Personal = {
            Name = "姓名",
            Alias = "别名",
            Race = "种族",
            Sex = "性别",
            Class = "职业",
            Age = "年龄",
            AgeCounter = function(years)
                return ("%s 岁"):format(years)
            end,
            Height = "身高",
            Cm = "厘米",
            Weight = "体重",
            Kg = "千克",
        },

        Trace = {
            Turns = "回合",
            TurnsCounter = function(turns)
                return ("%s回合"):format(turns)
            end,
            Days = "经过的天数",
            DaysCounter = function(days)
                return ("%s天"):format(days)
            end,
            Kills = "杀敌数",
            Time = "总时间",
        },

        Extra = {
            CargoWeight = "货物重量",
            CargoLimit = "货物限制",
            EquipWeight = "装备重量",
            DeepestLevel = "最深到达",
            DeepestLevelCounter = function(level)
                return ("相当于%s层"):format(level)
            end,
        },
    },
}