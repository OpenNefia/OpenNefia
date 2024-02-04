OpenNefia.Prototypes.Elona.Skill.Elona = {
Alchemy = {
        Description = "通过混合各种材料制作药剂",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s提高炼金技能"):format(_.kare_wa(item))
        end,
        Name = "炼金术",
    },
    Anatomy = {
        Description = "使尸体更容易保存",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s使尸体更容易保存"):format(_.kare_wa(item))
        end,
        Name = "解剖学",
    },
    Axe = {
        Description = "操控斧头的技术",
        Name = "斧",
        Damage = {
            WeaponName = "斧头",
            VerbPassive = "被砍",
            VerbActive = "砍",
        },
    },
    Blunt = {
        Description = "操控钝器的技术",
        Name = "钝器",
        Damage = {
            WeaponName = "钝器",
            VerbPassive = "被击中",
            VerbActive = "击打",
        },
    },
    Bow = {
        Description = "操控弓的技术",
        Name = "弓",
        Damage = {
            WeaponName = "弓",
            VerbPassive = "被射中",
            VerbActive = "射击",
        },
    },
    Carpentry = {
        Description = "加工木材并制作物品",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s提高木工技能"):format(_.kare_wa(item))
        end,
        Name = "木工",
    },
    Casting = {
        Description = "提高魔法咏唱的成功率",
        Name = "咏唱",
    },
    ControlMagic = {
        Description = "减少魔法对队友的影响",
        Name = "魔法控制",
    },Cooking = {
    Description = "提升烹饪技能",
    EnchantmentDescription = function(item, wielder, power)
        return ("%s提升烹饪技能"):format(_.kare_wa(item))
    end,
    Name = "烹饪",
},
Crossbow = {
    Description = "操纵弩技术",
    Name = "弩",
    Damage = {
        WeaponName = "弩",
        VerbPassive = "被射中",
        VerbActive = "射击",
    },
},
Detection = {
    Description = "发现隐藏的地点和陷阱",
    EnchantmentDescription = function(item, wielder, power)
        return ("%s增强侦查能力"):format(_.kare_wa(item))
    end,
    Name = "侦查",
},
DisarmTrap = {
    Description = "解除复杂的陷阱",
    EnchantmentDescription = function(item, wielder, power)
        return ("%s易于解除陷阱"):format(_.kare_wa(item))
    end,
    Name = "解除陷阱",
},
DualWield = {
    Description = "使用多把武器的技术",
    Name = "二刀流",
},
Evasion = {
    Description = "躲避攻击",
    Name = "闪避",
},
EyeOfMind = {
    Description = "提高暴击率",
    EnchantmentDescription = function(item, wielder, power)
        return ("%s提升心眼技术"):format(_.kare_wa(item))
    end,
    Name = "心眼",
},
Faith = {
    Description = "与神更加接近",
    EnchantmentDescription = function(item, wielder, power)
        return ("%s加深信仰"):format(_.kare_wa(item))
    end,
    Name = "信仰",
},Firearm = {
        Description = "远程机械武器技术",
        Name = "枪械",
        Damage = {
            WeaponName = "枪",
            VerbPassive = "被射击",
            VerbActive = "射击",
        },
    },
    Fishing = {
        Description = "钓鱼技术",
        EnchantmentDescription = function(item, wielder, power)
            return ("提高%s钓鱼技巧"):format(item)
        end,
        Name = "钓鱼",
    },
    Gardening = {
        Description = "种植和采集植物",
        EnchantmentDescription = function(item, wielder, power)
            return ("提高%s种植技巧"):format(item)
        end,
        Name = "种植",
    },
    GeneEngineer = {
        Description = "提升合成伙伴的知识",
        EnchantmentDescription = function(item, wielder, power)
            return ("深化%s基因工程知识"):format(item)
        end,
        Name = "基因工程",
    },
    GreaterEvasion = {
        Description = "确保躲避不准确的攻击",
        EnchantmentDescription = function(item, wielder, power)
            return ("提高%s躲闪技巧"):format(item)
        end,
        Name = "躲闪",
    },
    Healing = {
        Description = "自然地治愈伤口",
        EnchantmentDescription = function(item, wielder, power)
            return ("强化%s生命恢复"):format(item)
        end,
        Name = "治愈",
    },
    HeavyArmor = {
        Description = "熟练使用重型装备",
        EnchantmentDescription = function(item, wielder, power)
            return ("提高%s重型装备技术"):format(item)
        end,
        Name = "重型装备",
    },
    Investing = {
        Description = "有效投资",
        Name = "投资",
    },Jeweler = {
        Description = "加工宝石，制作物品",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s提升宝石工艺技能"):format(_.kare_wa(item))
        end,
        Name = "宝石工艺",
    },
    LightArmor = {
        Description = "轻型装备技术",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s提升轻型装备技能"):format(_.kare_wa(item))
        end,
        Name = "轻型装备",
    },
    Literacy = {
        Description = "解读难解书籍",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s增加对书籍的理解"):format(_.kare_wa(item))
        end,
        Name = "读书",
    },
    LockPicking = {
        Description = "开锁",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s增强开锁能力"):format(_.kare_wa(item))
        end,
        Name = "开锁",
    },
    LongSword = {
        Description = "使用长剑的技术",
        Name = "长剑",
        Damage = {
            WeaponName = "长剑",
            VerbPassive = "被切割",
            VerbActive = "挥舞",
        },
    },
    MagicCapacity = {
        Description = "保护自身免受魔法反噬",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s提升魔力极限"):format(_.kare_wa(item))
        end,
        Name = "魔力极限",
    },
    MagicDevice = {
        Description = "从道具中有效地提取魔力",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s提升魔法器具效果"):format(_.kare_wa(item))
        end,
        Name = "魔法器具",
    },
    Marksman = {
        Description = "提升射击威力",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s加深射击理解"):format(_.kare_wa(item))
        end,
        Name = "射击",
    },MartialArts = {
    Description = "格斗技术",
    Name = "格斗",
},
Meditation = {
    Description = "恢复消耗的魔力",
    EnchantmentDescription = function(item, wielder, power)
        return ("增强%s的魔力恢复"):format(_.kare_wa(item))
    end,
    Name = "冥想",
},
MediumArmor = {
    Description = "处理普通装备的技术",
    EnchantmentDescription = function(item, wielder, power)
        return ("增强%s中装备的技术"):format(_.kare_wa(item))
    end,
    Name = "中型装备",
},
Memorization = {
    Description = "记忆从书本中获得的知识",
    EnchantmentDescription = function(item, wielder, power)
        return ("防止%s的魔法知识遗忘"):format(_.kare_wa(item))
    end,
    Name = "记忆",
},
Mining = {
    Description = "提升挖掘墙壁的效率",
    EnchantmentDescription = function(item, wielder, power)
        return ("增强%s的挖掘能力"):format(_.kare_wa(item))
    end,
    Name = "挖掘",
},
Negotiation = {
    Description = "有利地进行交涉和商谈",
    EnchantmentDescription = function(item, wielder, power)
        return ("使%s交涉更有利"):format(_.kare_wa(item))
    end,
    Name = "交涉",
},
Performer = {
    Description = "实现高品质的演奏",
    EnchantmentDescription = function(item, wielder, power)
        return ("提升%s的演奏品质"):format(_.kare_wa(item))
    end,
    Name = "演奏",
},
Pickpocket = {
    Description = "偷取贵重物品",
    EnchantmentDescription = function(item, wielder, power)
        return ("提升%s的扒窃技巧"):format(_.kare_wa(item))
    end,
    Name = "扒窃",
},
Polearm = {
    Description = "槍術技巧",
    Name = "槍",
    Damage = {
        WeaponName = "槍",
        VerbPassive = "刺中",
        VerbActive = "突刺",
    },
},
Riding = {
    Description = "熟練操縦",
    EnchantmentDescription = function(item, wielder, power)
        return ("%s提升騎術技能"):format(_.kare_wa(item))
    end,
    Name = "騎術",
},
Scythe = {
    Description = "鐮刀技巧",
    Name = "鐮刀",
    Damage = {
        WeaponName = "鐮刀",
        VerbPassive = "斬傷",
        VerbActive = "斬擊",
    },
},
SenseQuality = {
    Description = "感知物品品質與種類",
    Name = "自然鑑定",
},
Shield = {
    Description = "盾牌技巧",
    Name = "盾",
},
ShortSword = {
    Description = "短劍技巧",
    Name = "短劍",
    Damage = {
        WeaponName = "短劍",
        VerbPassive = "刺中",
        VerbActive = "突刺",
    },
},
Stave = {
    Description = "法杖技巧",
    Name = "法杖",
    Damage = {
        WeaponName = "法杖",
        VerbPassive = "擊打",
        VerbActive = "打擊",
    },
},
Stealth = {
    Description = "悄悄行動",
    EnchantmentDescription = function(item, wielder, power)
        return ("%s強化潛行能力"):format(_.kare_wa(item))
    end,
    Name = "潛行",
},
Tactics = {
        Description = "增加近战攻击的威力",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s深入理解战术"):format(_.kare_wa(item))
        end,
        Name = "战术",
    },
    Tailoring = {
        Description = "使用皮革和藤蔓制作物品",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s提高裁缝技能"):format(_.kare_wa(item))
        end,
        Name = "裁缝",
    },
    Throwing = {
        Description = "掌握投掷物品的技巧",
        Name = "投掷",
        Damage = {
            WeaponName = "飞行道具",
            VerbPassive = "被攻击",
            VerbActive = "扔",
            AttacksActive = function(attacker, verb, target, weapon)
                return ("%s%s向%s%s%s"):format(_.kare_wa(attacker), _.name(target), _.name(weapon), verb)
            end,
        },
    },
    Traveling = {
        Description = "加快旅行进程并深化经验",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s提升旅行熟练度"):format(_.kare_wa(item))
        end,
        Name = "旅行",
    },
    TwoHand = {
        Description = "掌握双手武器技巧",
        Name = "双手持",
    },
    WeightLifting = {
        Description = "能够携带重物",
        Name = "举重",
    },
}