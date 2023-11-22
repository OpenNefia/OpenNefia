Elona.UI.ChooseAlly = {
    Window = {
        Title = "仲間",
        Proceed = "決定",
    },
    Topic = {
        Name = "仲間の情報",
        Status = "状態",
    },
    Status = {
        Alive = "(生きている)",
        Dead = "(死んでいる)",
        Waiting = "待機",
    },

    Call = {
        Window = {
            Title = "呼び戻す仲間",
        },
        Status = {
            Waiting = "(待機している)",
        },
        Prompt = "誰を呼び戻す？",
    },

    GeneEngineer = {
        Window = {
            Title = "仲間",
        },
        Topic = {
            BodySkill = "獲得部位/技能",
        },
        Status = {
            None = "なし",
        },
        Prompt = "対象にする仲間は？",
        SkillTooLow = "遺伝子学のスキルが足りない。",
    },

    PetArena = {
        Window = {
            Title = "出場する仲間",
        },
        Status = {
            In = " *出場* ",
        },
        IsDead = function(entity)
            return ("%s死んでいる。"):format(_.kare_wa(entity))
        end,
        NeedAtLeastOne = "最低でも一人の参加者が必要だ。",
        Prompt = "試合の規定人数",
        TooMany = "参加枠を超えている。",
    },

    Ranch = {
        Window = {
            Title = "ブリーダー候補",
        },
        Topic = {
            BreedPower = "繁殖力",
        },
        Prompt = "誰をブリーダーにする？",
    },

    Sell = {
        Window = {
            Title = "売り飛ばす仲間",
        },
        Topic = {
            Value = "値段",
        },
        Prompt = "誰を売り飛ばす？",
    },

    Shop = {
        Window = {
            Title = "店長候補",
        },
        Topic = {
            ChrNegotiation = "魅力/交渉",
        },
        Prompt = "誰を店長にする？",
    },
}
