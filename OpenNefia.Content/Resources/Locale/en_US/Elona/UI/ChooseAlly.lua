Elona.UI.ChooseAlly = {
    Window = {
        Title = "Ally List",
        Proceed = "Proceed",
    },
    Topic = {
        Name = "Name",
        Status = "Status",
    },
    Status = {
        Alive = "(Alive)",
        Dead = "(Dead)",
        Waiting = "Waiting",
    },

    Call = {
        Window = {
            Title = "Ally List",
        },
        Status = {
            Waiting = "(Waiting)",
        },
        Prompt = "Call who?",
    },

    GeneEngineer = {
        Window = {
            Title = "Ally List",
        },
        Topic = {
            BodySkill = "Body/Skill",
        },
        Status = {
            None = "None",
        },
        Prompt = "Who is the subject?",
        SkillTooLow = "You need to be a better gene engineer.",
    },

    PetArena = {
        Window = {
            Title = "Ally List",
        },
        Status = {
            In = "In",
        },
        IsDead = function(entity)
            return ("%s %s dead."):format(_.he(entity), _.is(entity))
        end,
        NeedAtLeastOne = "You need at least 1 pet to start the battle.",
        Prompt = "Participant",
        TooMany = "Too many participants.",
    },

    Ranch = {
        Window = {
            Title = "Ally List",
        },
        Topic = {
            BreedPower = "Breed Power",
        },
        Prompt = "Who takes the role of breeder?",
    },

    Sell = {
        Window = {
            Title = "Ally List",
        },
        Topic = {
            Value = "Value",
        },
        Prompt = "Sell who?",
    },

    Shop = {
        Window = {
            Title = "Ally List",
        },
        Topic = {
            ChrNegotiation = "CHR/Negotiation",
        },
        Prompt = "Who takes the role of shopkeeper?",
    },
}
