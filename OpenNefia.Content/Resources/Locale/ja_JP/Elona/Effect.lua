Elona.Effect = {
    Common = {
        ItIsCursed = "これは呪われている！",
        CursedLaughter = function(target)
            return ("%sは悪魔が笑う声を聞いた。"):format(_.name(target))
        end,
        Resists = function(target)
            return ("%sは抵抗した。"):format(_.name(target))
        end,
        CursedConsumable = function(target)
            return ("%sは気分が悪くなった。"):format(_.name(target))
        end,
    },

    Heal = {
        Apply = {
            Slightly = function(target)
                return ("%sの傷はふさがった。"):format(_.name(target))
            end,
            Normal = function(target)
                return ("%sは回復した。"):format(_.name(target))
            end,
            Greatly = function(target)
                return ("%sの身体に生命力がみなぎった。"):format(_.name(target))
            end,
            Completely = function(target)
                return ("%sは完全に回復した。"):format(_.name(target))
            end,
        },
    },

    HealSanity = {
        RainOfSanity = function(target)
            return ("%sの狂気は消え去った。"):format(_.name(target))
        end,
    },

    Identify = {
        Fully = function(item)
            return ("それは%sだと完全に判明した。"):format(_.name(item))
        end,
        Partially = function(item)
            return ("それは%sだと判明したが、完全には鑑定できなかった。"):format(_.name(item))
        end,
        NeedMorePower = "新しい知識は得られなかった。より上位の鑑定で調べる必要がある。",
    },

    Curse = {
        Action = function(source, target)
            return ("%sは%sを指差して呪いの言葉を呟いた。"):format(_.name(source), _.name(target))
        end,
        Apply = function(source, target, item)
            return ("%sの%sは黒く輝いた。"):format(_.name(target), _.name(item, true, 1))
        end,
        NoEffect = function(source, target)
            return ("%sは祈祷を捧げ呪いのつぶやきを無効にした。"):format(_.name(source))
        end,
    },

    Teleport = {
        Prevented = "魔法の力がテレポートを防いだ。",
        General = function(ent)
            return ("%sは突然消えた。"):format(_.name(ent, true))
        end,
        DrawShadow = function(ent)
            return ("%sは引き寄せられた。"):format(_.name(ent, true))
        end,
        ShadowStep = function(ent, source, target)
            return ("%sは%sの元に移動した。"):format(_.name(source, true), _.basename(target))
        end,
    },

    GainAlly = {
        YoungerSister = "なんと、あなたには生き別れた血の繋がっていない妹がいた！",
        YoungLady = "お嬢さんが空から降ってきた！",
        CatSister = "なんと、あなたには生き別れた血の繋がっていないぬこの妹がいた！",
    },

    Oracle = {
        Cursed = "何かがあなたの耳元でささやいたが、あなたは聞き取ることができなかった。",
        NoArtifactsYet = "まだ特殊なアイテムは生成されていない。",
    },

    Uncurse = {
        Power = {
            Normal = function(source, target)
                return ("%sの装備品は白い光に包まれた。"):format(_.name(target))
            end,
            Blessed = function(source, target)
                return ("%sは聖なる光に包み込まれた。"):format(_.name(target))
            end,
        },
        Apply = {
            Equipment = "身に付けている装備の幾つかが浄化された。",
            Item = "幾つかのアイテムが浄化された。",
            Resisted = "幾つかのアイテムは抵抗した。",
        },
    },

    WallCreation = {
        WallAppears = "床が盛り上がってきた。",
    },

    DoorCreation = {
        WallsResist = "この壁は魔法を受け付けないようだ。",
        DoorAppears = "扉が出現した。",
    },

    WizardsHarvest = {
        FallsDown = function(source, item)
            return ("%sが降ってきた！"):format(_.name(item), _.s(item))
        end,
    },

    Restore = {
        Body = {
            Apply = function(source, target)
                return ("%sの肉体は復活した。"):format(_.name(target))
            end,
            Blessed = function(source, target)
                return ("さらに、%sの肉体は強化された。"):format(_.name(target))
            end,
            Cursed = function(source, target)
                return ("%sの肉体は蝕まれた。"):format(_.name(target))
            end,
        },
        Spirit = {
            Apply = function(source, target)
                return ("%sの精神は復活した。"):format(_.name(target))
            end,
            Blessed = function(source, target)
                return ("さらに、%sの精神は強化された。"):format(_.name(target))
            end,
            Cursed = function(source, target)
                return ("%sの精神は蝕まれた。"):format(_.name(target))
            end,
        },
    },

    Mutation = {
        Apply = function(source, target)
            return ("%sは変容した！ "):format(_.name(target))
        end,
        Resist = function(source, target)
            return ("%sは変異を受け付けなかった。"):format(_.name(target))
        end,
        Eye = function(source, target)
            return ("%sは%sを気の狂いそうな眼差しで見た。"):format(_.name(source), _.name(_target))
        end,
    },

    CureMutation = {
        Message = function(source, target)
            return ("%sは元の自分に近づいた気がした。"):format(_.name(target))
        end,
    },

    Domination = {
        CannotBeCharmed = function(source, target)
            return ("%sは支配できない。"):format(_.name(target))
        end,
        DoesNotWorkHere = "この場所では効果がない。",
    },

    Resurrection = {
        Cursed = "冥界から死霊が呼び出された！",

        Prompt = "誰を蘇らせる？",

        Apply = function(source, target)
            return ("%sは復活した！"):format(_.name(target))
        end,
        Fail = function(source, target)
            return ("%sの力は冥界に及ばなかった。"):format(_.name(source))
        end,

        Dialog = _.quote "ありがとう！",
    },

    Sense = {
        Cursed = function(source, target)
            return ("あれ…？%sは軽い記憶障害を受けた。"):format(source)
        end,

        MagicMap = function(source, target)
            return ("%sは周囲の地形を察知した。"):format(name(_1))
        end,

        SenseObject = function(source, target)
            return ("%sは周囲の物質を感知した。"):format(_.name(source))
        end,
    },

    FourDimensionalPocket = {
        Summon = "あなたは四次元のポケットを召喚した。",
    },
}
