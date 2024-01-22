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
            Slightly = function(source, target)
                return ("%sの傷はふさがった。"):format(_.name(target))
            end,
            Normal = function(source, target)
                return ("%sは回復した。"):format(_.name(target))
            end,
            Greatly = function(source, target)
                return ("%sの身体に生命力がみなぎった。"):format(_.name(target))
            end,
            Completely = function(source, target)
                return ("%sは完全に回復した。"):format(_.name(target))
            end,
        },
    },

    HealMP = {
        Normal = function(source, target)
            return ("%sのマナが回復した。"):format(_.name(target))
        end,
        AbsorbMagic = function(source, target)
            return ("%sは周囲からマナを吸い取った。"):format(_.name(target))
        end,
    },

    HealSanity = {
        RainOfSanity = function(source, target)
            return ("%sの狂気は消え去った。"):format(_.name(target))
        end,
    },

    MObj = {
        Drops = function(source, target, mobj)
            return ("%sは%sを投下した。"):format(_.name(source), _.name(mobj))
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

    Meteor = {
        Falls = "隕石が落ちてきた！",
    },

    DrainBlood = {
        Ally = function(source, target)
            return ("%s%sに血を吸われた。"):format(_.sore_wa(source), _.name(target))
        end,
        Other = function(source, target)
            return ("%s%sの血を吸い"):format(_.sore_wa(source), _.name(target))
        end,
    },

    TouchOfWeakness = {
        Apply = function(source, target)
            return ("%sは弱くなった。"):format(_.name(target))
        end,
    },

    TouchOfHunger = {
        Apply = function(source, target)
            return ("%sはお腹が減った。"):format(_.name(target))
        end,
    },

    ManisDisassembly = {
        Dialog = _.quote "余分な機能は削除してしまえ",
    },

    Mirror = {
        Examine = function(source, target)
            return ("%sは%sの状態を調べた。"):format(_.name(source), _.theTarget(source, target))
        end,
    },

    Change = {
        Changes = function(source, target)
            return ("%sは変化した。"):format(_.name(target))
        end,
        CannotBeChanged = function(source, target)
            return ("%sは変化できない。"):format(_.name(target))
        end,
    },

    Swarm = {
        Apply = "スウォーム！",
    },

    SuspiciousHand = {
        GuardsWallet = function(source, target)
            return ("%sは自分の財布を守った。"):format(_.name(target))
        end,
        Steals = function(source, target, goldStolen)
            return ("%sは%sから%s枚の金貨を奪った。"):format(_.name(source), _.name(target), goldStolen)
        end,
        Escapes = "泥棒は笑って逃げた。",
    },

    EyeOfInsanity = {
        Message = {
            function(source, target)
                return ("%sは%sの腹の亀裂から蛆虫が沸き出るのを見た。"):format(
                    _.name(target),
                    _.name(source)
                )
            end,
            function(source, target)
                return ("%sは%sが屍を貪る姿を目撃した。"):format(_.name(target), _.name(source))
            end,
            function(source, target)
                return ("%sは%sの恐ろしい瞳に震えた。"):format(_.name(target), _.name(source))
            end,
            function(source, target)
                return ("%sは%sの触手に絡まる臓物に吐き気を感じた。"):format(
                    _.name(target),
                    _.name(source)
                )
            end,
        },
    },

    EyeOfMana = {
        Apply = function(source, target)
            return ("%sは%sを睨み付けた。"):format(_.name(source), _.name(target))
        end,
    },

    SuicideAttack = {
        Explodes = function(source, target)
            return ("%sは爆発した。"):format(_.name(source))
        end,
        ChainExplodes = function(source, target)
            return ("%sは誘爆した。"):format(_.name(source))
        end,
        ExplosionHits = {
            Ally = function(source, target)
                return ("爆風が%sに命中した。"):format(_.name(target))
            end,
            Other = function(source, target)
                return ("爆風は%sに命中し"):format(_.name(target))
            end,
        },
    },

    Insult = {
        Apply = function(source, target)
            return ("%s insult%s %s."):format(_.name(source), _.s(source), _.name(target))
        end,
        Insults = {
            Male = {
                _.quote "すっこんでろ雑魚め",
                _.quote "オマエ程度が戦うだと？",
                _.quote "すぐに殺してやるよ",
                _.quote "消えろザコめ",
                _.quote "このかたつむり野郎",
                _.quote "すぐにミンチにしてやるよ",
            },
            Female = {
                _.quote "グシャグシャにしてやるわ",
                _.quote "地べたを這いずりなさい",
                _.quote "ウージッムシ♪ウージッムシ♪",
                _.quote "目障りよ",
                _.quote "もがけ。苦しめ！",
                _.quote "その下品な眼をくりぬくの",
                _.quote "このカタツムリが",
                _.quote "どうしたの？もう終わりなの？",
                _.quote "潔く、くたばりなさい",
                _.quote "生まれてきたことを後悔するのね",
                _.quote "このブタめ",
                _.quote "すぐにミンチにしてあげる",
            },
        },
    },

    Scavenge = {
        Apply = function(source, target)
            return ("%sは%sのバックパックを漁った。"):format(_.name(source), _.name(target))
        end,
        Spiked = function(source, target, item)
            return ("%sは%sの異臭に気付き手をひっこめた。"):format(_.name(source), _.name(item))
        end,
        Eats = function(source, target, item)
            return ("%sは%sを食べた！"):format(_.name(source), _.name(item))
        end,
    },

    Vanish = {
        Vanishes = function(source, target)
            return ("%sは消え去った。"):format(_.name(target))
        end,
    },

    Cheer = {
        Cheers = function(source, target)
            return ("%sは仲間を鼓舞した。"):format(_.name(source))
        end,
        IsExcited = function(source, target)
            return ("%sは興奮した！"):format(_.name(target))
        end,
    },

    MewMewMew = {
        Message = "うみみゃぁ！",
    },

    Decapitation = {
        Sound = " *ブシュッ* ",
        Apply = {
            Ally = function(source, target)
                return ("%sは%sの首をちょんぎった。"):format(_.name(source), _.name(target))
            end,
            Other = function(source, target)
                return ("%sは%sの首をちょんぎり"):format(_.name(source), _.name(target))
            end,
        },
    },
}
