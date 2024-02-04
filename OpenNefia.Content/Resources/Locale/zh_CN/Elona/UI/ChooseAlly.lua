Elona.UI.ChooseAlly = {
    Window = {
        Title = "伙伴",
        Proceed = "确定",
    },
    Topic = {
        Name = "仲间的信息",
        Status = "状态",
    },
    Status = {
        Alive = "(存活)",
        Dead = "(已死亡)",
        Waiting = "等待中",
    },

    Call = {
        Window = {
            Title = "召回的伙伴",
        },
        Status = {
            Waiting = "(等待中)",
        },
        Prompt = "召回谁？",
    },

    GeneEngineer = {
        Window = {
            Title = "伙伴",
        },
        Topic = {
            BodySkill = "获得部位/技能",
        },
        Status = {
            None = "无",
        },
        Prompt = "选择目标的伙伴？",
        SkillTooLow = "遗传学技能不足。",
    },

    PetArena = {
        Window = {
            Title = "参战的伙伴",
        },
        Status = {
            In = " *参战* ",
        },
        IsDead = function(entity)
            return (" %s 已死亡。"):format(_.kare_wa(entity))
        end,
        NeedAtLeastOne = "必须至少有一个参与者。",
        Prompt = "比赛规定人数",
        TooMany = "超过参与者名额。",
    },

    Ranch = {
        Window = {
            Title = "驯养师候选",
        },
        Topic = {
            BreedPower = "繁殖力",
        },
        Prompt = "选择谁作为驯养师？",
    },

    Sell = {
        Window = {
            Title = "出售的伙伴",
        },
        Topic = {
            Value = "价格",
        },
        Prompt = "卖给谁？",
    },

    Shop = {
        Window = {
            Title = "商店经理候选",
        },
        Topic = {
            ChrNegotiation = "魅力/交涉",
        },
        Prompt = "任命谁为商店经理？",
    },
}