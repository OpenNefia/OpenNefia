OpenNefia.Prototypes.Elona.Element.Elona = {
    Chaos = {
        Name = "混沌",
        ShortName = "沌",
        Description = "对混沌效果的抵抗力",
        Ego = "混沌的",
        Resist = {
            Gain = function(_1)
                return ("%s不再担心噪音。"):format(_.name(_1))
            end,
            Lose = function(_1)
                return ("%s失去了对混沌的理解。"):format(_.name(_1))
            end,
        },
        Wounded = function(_1)
            return ("%s受到混沌漩涡的伤害。"):format(_.name(_1))
        end,
        Killed = {
            Active = "被混沌漩涡吸入。",
            Passive = function(_1)
                return ("%s被混沌漩涡吸收。"):format(_.name(_1))
            end,
        },
    },
    Cold = {
        Name = "冷气",
        ShortName = "冷",
        Description = "对冷气和水的抵抗力",
        Ego = "冷的",
        Resist = {
            Gain = function(_1)
                return ("%s的身体突然变得冰冷。"):format(_.name(_1))
            end,
            Lose = function(_1)
                return ("%s突然感到寒意。"):format(_.name(_1))
            end,
        },
        Wounded = function(_1)
            return ("%s冻结了。"):format(_.name(_1))
        end,
        Killed = {
            Active = "变成了冰块。",
            Passive = function(_1)
                return ("%s变成了冰雕。"):format(_.name(_1))
            end,
        },
    },
    Cut = {
        Name = "出血",
        Description = "对切伤的抵抗力",
        Ego = "出血的",
        Wounded = function(_1)
            return ("%s受到了切伤。"):format(_.name(_1))
        end,
        Killed = {
            Active = "切成了碎片。",
            Passive = function(_1)
                return ("%s切成了碎片。"):format(_.name(_1))
            end,
        },
    },
    Darkness = {
        Name = "黑暗",
        ShortName = "暗",
        Description = "对黑暗和失明的抵抗力",
        Ego = "黑暗的",
        Resist = {
            Gain = function(_1)
                return ("%s突然不再害怕黑暗。"):format(_.name(_1))
            end,
            Lose = function(_1)
                return ("%s突然害怕黑暗。"):format(_.name(_1))
            end,
        },
        Wounded = function(_1)
            return ("%s受到了黑暗力量的伤害。"):format(_.name(_1))
        end,
        Killed = {
            Active = "被黑暗吞噬。",
            Passive = function(_1)
                return ("%s被黑暗侵蚀而死。"):format(_.name(_1))
            end,
        },
    },
    Fire = {
        Name = "火炎",
        ShortName = "火",
        Description = "对热和火焰的抵抗力",
        Ego = "燃烧的",
        Resist = {
            Gain = function(_1)
                return ("%s的身体突然变得火热。"):format(_.name(_1))
            end,
            Lose = function(_1)
                return ("%s突然开始出汗。"):format(_.name(_1))
            end,
        },
        Wounded = function(_1)
            return ("%s被烧伤了。"):format(_.name(_1))
        end,
        Killed = {
            Active = "烧成灰烬。",
            Passive = function(_1)
                return ("%s被烧尽变成灰烬。"):format(_.name(_1))
            end,
        },
    },
    Lightning = {
        Name = "电击",
        ShortName = "雷",
        Description = "对雷电和电击的抵抗力",
        Ego = "放电的",
        Resist = {
            Gain = function(_1)
                return ("%s的身体传来电流。"):format(_.name(_1))
            end,
            Lose = function(_1)
                return ("%s突然对电流敏感起来。"):format(_.name(_1))
            end,
        },
        Wounded = function(_1)
            return ("%s被电流击中。"):format(_.name(_1))
        end,
        Killed = {
            Active = "烧成灰烬。",
            Passive = function(_1)
                return ("%s被雷电击中而死。"):format(_.name(_1))
            end,
        },
    },
    Magic = {
        Name = "魔法",
        ShortName = "魔",
        Description = "对魔法攻击的抵抗力",
        Resist = {
            Gain = function(_1)
                return ("%s的皮肤被魔力的光环包裹。"):format(_.name(_1))
            end,
            Lose = function(_1)
                return ("%s的皮肤上的魔力光环消失了。"):format(_.name(_1))
            end,
        },
    },
    Mind = {
        Name = "幻觉",
        ShortName = "幻",
        Description = "对精神攻击的抵抗力",
        Ego = "灵性的",
        Resist = {
            Gain = function(_1)
                return ("%s突然变得清晰起来。"):format(_.name(_1))
            end,
            Lose = function(_1)
                return ("%s不再像以前那样清晰。"):format(_.name(_1))
            end,
        },
        Wounded = function(_1)
            return ("%s受到了疯狂的困扰。"):format(_.name(_1))
        end,
        Killed = {
            Active = "使其无法复活。",
            Passive = function(_1)
                return ("%s发疯而死。"):format(_.name(_1))
            end,
        },
    },Nerve = {
        Name = "神経",
        ShortName = "神",
        Description = "对睡眠和麻痹具有耐性",
        Ego = "麻痹的",
        Resist = {
            Gain = function(_1)
                return ("%s的神经突然变得坚韧起来。"):format(_.name(_1))
            end,
            Lose = function(_1)
                return ("%s的神经突然萎缩了。"):format(_.name(_1))
            end,
        },
        Wounded = function(_1)
            return ("%s的神经受伤了。"):format(_.name(_1))
        end,
        Killed = {
            Active = "破坏了神经。",
            Passive = function(_1)
                return ("%s的神经受到侵蚀而死亡。"):format(_.name(_1))
            end,
        },
    },
    Nether = {
        Name = "地狱",
        ShortName = "狱",
        Description = "对生命吸收具有抵抗力",
        Ego = "地狱的",
        Resist = {
            Gain = function(_1)
                return ("%s的灵魂靠近了地狱。"):format(_.name(_1))
            end,
            Lose = function(_1)
                return ("%s的灵魂远离了地狱。"):format(_.name(_1))
            end,
        },
        Wounded = function(_1)
            return ("%s受到了冥界的冷气伤害。"):format(_.name(_1))
        end,
        Killed = {
            Active = "坠入冥界。",
            Passive = function(_1)
                return ("%s堕入冥界。"):format(_.name(_1))
            end,
        },
    },
    Poison = {
        Name = "毒",
        ShortName = "毒",
        Description = "对毒素具有耐性",
        Ego = "毒的",
        Resist = {
            Gain = function(_1)
                return ("%s对毒素的耐性变得更强了。"):format(_.name(_1))
            end,
            Lose = function(_1)
                return ("%s对毒素的耐性减弱了。"):format(_.name(_1))
            end,
        },
        Wounded = function(_1)
            return ("%s感到恶心。"):format(_.name(_1))
        end,
        Killed = {
            Active = "毒杀了。",
            Passive = function(_1)
                return ("%s被毒素侵蚀而死亡。"):format(_.name(_1))
            end,
        },
    },
    Sound = {
        Name = "音",
        ShortName = "音",
        Description = "对声波和轰鸣具有耐性",
        Ego = "颤抖的",
        Resist = {
            Gain = function(_1)
                return ("%s不再对噪音感到困扰。"):format(_.name(_1))
            end,
            Lose = function(_1)
                return ("%s突然觉得周围很吵。"):format(_.name(_1))
            end,
        },
        Wounded = function(_1)
            return ("%s受到轰鸣的冲击。"):format(_.name(_1))
        end,
        Killed = {
            Active = "破坏了听觉并杀死了。",
            Passive = function(_1)
                return ("%s变得模糊不清地死去。"):format(_.name(_1))
            end,
        },
    },
    Ether = {
        Ego = "乙醚的",
    },
    Acid = {
        Wounded = function(_1)
            return ("%s被酸液灼烧。"):format(_.name(_1))
        end,
        Killed = {
            Active = "溶解成一团糊状物。",
            Passive = function(_1)
                return ("%s被酸液灼烧并溶化了。"):format(_.name(_1))
            end,
        },
    },
    Rotten = {
        Ego = "腐烂的",
    },
    Hunger = {
        Ego = "饥饿的",
    },
    Fear = {
        Ego = "恐ろしい",
    },
    Soft = {
        Ego = "柔らかい",
    },
    Vorpal = {
        Ego = "ヴォーパル",
    },
}