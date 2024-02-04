Elona.Effect = {
Common = {
        ItIsCursed = "这个是被诅咒的！",
        CursedLaughter = function(target)
            return ("听到了%s恶魔的笑声。"):format(_.name(target))
        end,
        Resists = function(target)
            return ("%s抵抗了。"):format(_.name(target))
        end,
        CursedConsumable = function(target)
            return ("%s感到不舒服。"):format(_.name(target))
        end,
    },

    Heal = {
        Apply = {
            Slightly = function(source, target)
                return ("%s的伤口愈合了一点点。"):format(_.name(target))
            end,
            Normal = function(source, target)
                return ("%s恢复了。"):format(_.name(target))
            end,
            Greatly = function(source, target)
                return ("%s的身体充满了生命力。"):format(_.name(target))
            end,
            Completely = function(source, target)
                return ("%s完全恢复了。"):format(_.name(target))
            end,
        },
    },

    HealMP = {
        Normal = function(source, target)
            return ("%s的法力回复了。"):format(_.name(target))
        end,
        AbsorbMagic = function(source, target)
            return ("%s吸收了周围的法力。"):format(_.name(target))
        end,
    },

    HealSanity = {
        RainOfSanity = function(source, target)
            return ("%s的疯狂消失了。"):format(_.name(target))
        end,
    },

    MObj = {
        Drops = function(source, target, mobj)
            return ("%s投下了%s。"):format(_.name(source), _.name(mobj))
        end,
    },

    Identify = {
        Fully = function(item)
            return ("那是%s，已经完全鉴定。"):format(_.name(item))
        end,
        Partially = function(item)
            return ("那是%s，但无法完全鉴定。"):format(_.name(item))
        end,
        NeedMorePower = "没有获得新的知识。需要更高级的鉴定。",
    },

    Curse = {
        Action = function(source, target)
            return ("%s指着%s低声诅咒。"):format(_.name(source), _.name(target))
        end,
        Apply = function(source, target, item)
            return ("%s的%s变得黑光闪闪。"):format(_.name(target), _.name(item, true, 1))
        end,
        NoEffect = function(source, target)
            return ("%s献上祈祷，抵消了诅咒的低语。"):format(_.name(source))
        end,
    },

    Teleport = {
        Prevented = "魔法力量阻止了传送。",
        General = function(ent)
            return ("%s突然消失了。"):format(_.name(ent, true))
        end,
        DrawShadow = function(ent)
            return ("%s被吸引了过来。"):format(_.name(ent, true))
        end,
        ShadowStep = function(ent, source, target)
            return ("%s移动到了%s的位置。"):format(_.name(source, true), _.basename(target))
        end,
    },GainAlly = {
        YoungerSister = "哇，原来你有一位与你没有血缘关系的妹妹！",
        YoungLady = "一位小姐从天而降！",
        CatSister = "哇，原来你有一位与你没有血缘关系的猫妹妹！",
    },

    Oracle = {
        Cursed = "有什么东西在你耳边低语，但你无法听清。",
        NoArtifactsYet = "尚未生成特殊物品。",
    },

    Uncurse = {
        Power = {
            Normal = function(source, target)
                return ("%s的装备被白色光芒包裹。"):format(_.name(target))
            end,
            Blessed = function(source, target)
                return ("%s被圣洁的光芒包裹。"):format(_.name(target))
            end,
        },
        Apply = {
            Equipment = "身上戴着的一些装备被净化了。",
            Item = "一些物品被净化了。",
            Resisted = "一些物品抵抗了净化。",
        },
    },

    WallCreation = {
        WallAppears = "地板凸起了。",
    },

    DoorCreation = {
        WallsResist = "这堵墙不受魔法影响。",
        DoorAppears = "门出现了。",
    },

    WizardsHarvest = {
        FallsDown = function(source, item)
            return ("%s掉了下来！"):format(_.name(item), _.s(item))
        end,
    },

    Restore = {
        Body = {
            Apply = function(source, target)
                return ("%s的身体复活了。"):format(_.name(target))
            end,
            Blessed = function(source, target)
                return ("此外，%s的身体得到了强化。"):format(_.name(target))
            end,
            Cursed = function(source, target)
                return ("%s的身体被毁坏。"):format(_.name(target))
            end,
        },
        Spirit = {
            Apply = function(source, target)
                return ("%s的精神复活了。"):format(_.name(target))
            end,
            Blessed = function(source, target)
                return ("此外，%s的精神得到了强化。"):format(_.name(target))
            end,
            Cursed = function(source, target)
                return ("%s的精神受到侵蚀。"):format(_.name(target))
            end,
        },
    },

    Mutation = {
        Apply = function(source, target)
            return ("%s发生了变异！"):format(_.name(target))
        end,
        Resist = function(source, target)
            return ("%s抵抗了变异。"):format(_.name(target))
        end,
        Eye = function(source, target)
            return ("%s用疯狂的眼神盯着%s。"):format(_.name(source), _.name(target))
        end,
    },CureMutation = {
        Message = function(source, target)
            return ("%s觉得自己靠近了原本的自己。"):format(_.name(target))
        end,
    },

    Domination = {
        CannotBeCharmed = function(source, target)
            return ("%s无法被控制。"):format(_.name(target))
        end,
        DoesNotWorkHere = "在这个地方没有效果。",
    },

    Resurrection = {
        Cursed = "从冥界召唤了亡灵!",

        Prompt = "谁将复活？",

        Apply = function(source, target)
            return ("%s复活了！"):format(_.name(target))
        end,
        Fail = function(source, target)
            return ("%s的力量无法触及冥界。"):format(_.name(source))
        end,

        Dialog = _.quote "谢谢!",
    },

    Sense = {
        Cursed = function(source, target)
            return ("奇怪...？%s受到了轻度记忆障碍。"):format(source)
        end,

        MagicMap = function(source, target)
            return ("%s察觉到周围的地形。"):format(name(_1))
        end,

        SenseObject = function(source, target)
            return ("%s感知到周围的物质。"):format(_.name(source))
        end,
    },

    FourDimensionalPocket = {
        Summon = "你召唤出了四维口袋。",
    },

    Meteor = {
        Falls = "陨石落下了！",
    },

    DrainBlood = {
        Ally = function(source, target)
            return ("%s吸取了%s的血。"):format(_.sore_wa(source), _.name(target))
        end,
        Other = function(source, target)
            return ("%s吸取了%s的血液"):format(_.sore_wa(source), _.name(target))
        end,
    },

    TouchOfWeakness = {
        Apply = function(source, target)
            return ("%s变弱了。"):format(_.name(target))
        end,
    },TouchOfHunger = {
        Apply = function(source, target)
            return ("%s 饿了。"):format(_.name(target))
        end,
    },

    ManisDisassembly = {
        Dialog = _.quote "删除多余功能",
    },

    Mirror = {
        Examine = function(source, target)
            return ("%s 检查了 %s 的状态。"):format(_.name(source), _.theTarget(source, target))
        end,
    },

    Change = {
        Changes = function(source, target)
            return ("%s 发生了变化。"):format(_.name(target))
        end,
        CannotBeChanged = function(source, target)
            return ("%s 无法改变。"):format(_.name(target))
        end,
    },

    Swarm = {
        Apply = "成群结队！",
    },

    SuspiciousHand = {
        GuardsWallet = function(source, target)
            return ("%s 保护了自己的钱包。"):format(_.name(target))
        end,
        Steals = function(source, target, goldStolen)
            return ("%s 从 %s 偷走了 %s 金币。"):format(_.name(source), _.name(target), goldStolen)
        end,
        Escapes = "小偷笑着逃走了。",
    },

    EyeOfInsanity = {
        Message = {
            function(source, target)
                return ("%s 看到了从 %s 腹部裂缝中涌出的蛆虫。"):format(
                    _.name(target),
                    _.name(source)
                )
            end,
            function(source, target)
                return ("%s 目睹了 %s 剥食尸体的情景。"):format(_.name(target), _.name(source))
            end,
            function(source, target)
                return ("%s 震惊于 %s 恐怖的眼神。"):format(_.name(target), _.name(source))
            end,
            function(source, target)
                return ("%s 对 %s 触手缠绕的内脏感到恶心。"):format(
                    _.name(target),
                    _.name(source)
                )
            end,
        },
    },

    EyeOfMana = {
            Apply = function(source, target)
                return ("%s 盯着 %s 看。"):format(_.name(source), _.name(target))
            end,
        },
        SuicideAttack = {
        Explodes = function(source, target)
            return ("%s爆炸了。"):format(_.name(source))
        end,
        ChainExplodes = function(source, target)
            return ("%s引爆了。"):format(_.name(source))
        end,
        ExplosionHits = {
            Ally = function(source, target)
                return ("爆炸波击中了%s。"):format(_.name(target))
            end,
            Other = function(source, target)
                return ("爆炸波击中了%s").format(_.name(target))
            end,
        },
    },

    Insult = {
        Apply = function(source, target)
            return ("%s侮辱了%s。"):format(_.name(source), _.name(target))
        end,
        Insults = {
            Male = {
                _.quote "滚开，渣渣",
                _.quote "你以为你能战斗？",
                _.quote "我会立刻干掉你的",
                _.quote "消失吧，废物",
                _.quote "你这只蜗牛",
                _.quote "我会把你搅成碎片的",
            },
            Female = {
                _.quote "我会把你搅成糊状的",
                _.quote "趴在地上爬行吧",
                _.quote "嗡嗡嗡♪嗡嗡嗡♪",
                _.quote "碍眼",
                _.quote "挣扎吧。让你受点苦！",
                _.quote "我会挖掉那低级的眼睛",
                _.quote "这只蜗牛",
                _.quote "怎么了？结束了吗？",
                _.quote "勇敢地死去吧",
                _.quote "后悔自己出生了",
                _.quote "这只猪",
                _.quote "我会立刻把你弄成碎片",
            },
        },
    },

    Scavenge = {
        Apply = function(source, target)
            return ("%s搜查了%s的背包。"):format(_.name(source), _.name(target))
        end,
        Spiked = function(source, target, item)
            return ("%s闻到了%s的异味，缩回了手。"):format(_.name(source), _.name(item))
        end,
        Eats = function(source, target, item)
            return ("%s吃掉了%s！"):format(_.name(source), _.name(item))
        end,
    },

    Vanish = {
        Vanishes = function(source, target)
            return ("%s消失了。"):format(_.name(target))
        end,
    },

    Cheer = {
        Cheers = function(source, target)
            return ("%s鼓舞了队友。"):format(_.name(source))
        end,
        IsExcited = function(source, target)
            return ("%s兴奋了！"):format(_.name(target))
        end,
    },

    MewMewMew = {
        Message = "咪咪咪！",
    },

    Decapitation = {
        Sound = " *刷* ",
        Apply = {
            Ally = function(source, target)
                return ("%s砍掉了%s的头。"):format(_.name(source), _.name(target))
            end,
            Other = function(source, target)
                return ("%s砍掉了%s的头").format(_.name(source), _.name(target))
            end,
        },
    },
}
