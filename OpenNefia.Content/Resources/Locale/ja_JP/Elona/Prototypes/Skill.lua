OpenNefia.Prototypes.Elona.Skill.Elona = {
    AttrStrength = {
        Name = "筋力",
        ShortName = "筋力",
        OnDecrease = function(entity)
            return ("%sは少し贅肉が増えたような気がした。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sはより強くなった。"):format(_.name(entity))
        end,
    },
    AttrConstitution = {
        Name = "耐久",
        ShortName = "耐久",
        OnDecrease = function(entity)
            return ("%sは我慢ができなくなった。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは我慢することの快感を知った。"):format(_.name(entity))
        end,
    },
    AttrDexterity = {
        Name = "器用",
        ShortName = "器用",
        OnDecrease = function(entity)
            return ("%sは不器用になった。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは器用になった。"):format(_.name(entity))
        end,
    },
    AttrPerception = {
        Name = "感覚",
        ShortName = "感覚",
        OnDecrease = function(entity)
            return ("%sは感覚のずれを感じた。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは世界をより身近に感じるようになった。"):format(_.name(entity))
        end,
    },
    AttrLearning = {
        Name = "習得",
        ShortName = "習得",
        OnDecrease = function(entity)
            return ("%sの学習意欲が低下した。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは急に色々なことを学びたくなった。"):format(_.name(entity))
        end,
    },
    AttrWill = {
        Name = "意思",
        ShortName = "意思",
        OnDecrease = function(entity)
            return ("%sは何でもすぐ諦める。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sの意思は固くなった。"):format(_.name(entity))
        end,
    },
    AttrMagic = {
        Name = "魔力",
        ShortName = "魔力",
        OnDecrease = function(entity)
            return ("%sは魔力の衰えを感じた。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは魔力の上昇を感じた。"):format(_.name(entity))
        end,
    },
    AttrCharisma = {
        Name = "魅力",
        ShortName = "魅力",
        OnDecrease = function(entity)
            return ("%sは急に人前に出るのが嫌になった。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは周囲の視線を心地よく感じる。"):format(_.name(entity))
        end,
    },
    AttrSpeed = {
        Name = "速度",
        OnDecrease = function(entity)
            return ("%sは遅くなった。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは周りの動きが遅く見えるようになった。"):format(_.name(entity))
        end,
    },
    AttrLuck = {
        Name = "運勢",
        OnDecrease = function(entity)
            return ("%sは不幸になった。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは幸運になった。"):format(_.name(entity))
        end,
    },
    AttrLife = {
        Name = "生命力",
        OnDecrease = function(entity)
            return ("%sは生命力の衰えを感じた。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは生命力の上昇を感じた。"):format(_.name(entity))
        end,
    },
    AttrMana = {
        Name = "マナ",
        OnDecrease = function(entity)
            return ("%sはマナの衰えを感じた。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sはマナの向上を感じた。"):format(_.name(entity))
        end,
    },
    Alchemy = {
        Description = "様々な材料を調合し、ポーションを作り出す",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s錬金の腕を上げる"):format(_.kare_wa(item))
        end,
        Name = "錬金術",
    },
    Anatomy = {
        Description = "死体を残しやすくする",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s死体を残しやすくする"):format(_.kare_wa(item))
        end,
        Name = "解剖学",
    },
    Axe = {
        Description = "斧を扱う技術",
        Name = "斧",
        Damage = {
            WeaponName = "斧",
            VerbPassive = "切られた",
            VerbActive = "切り払い",
        },
    },
    Blunt = {
        Description = "鈍器を扱う技術",
        Name = "鈍器",
        Damage = {
            WeaponName = "鈍器",
            VerbPassive = "打たれた",
            VerbActive = "打って",
        },
    },
    Bow = {
        Description = "弓を扱う技術",
        Name = "弓",
        Damage = {
            WeaponName = "弓",
            VerbPassive = "撃たれた",
            VerbActive = "射撃し",
        },
    },
    Carpentry = {
        Description = "木を加工し、アイテムを作り出す",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s大工の腕を上げる"):format(_.kare_wa(item))
        end,
        Name = "大工",
    },
    Casting = {
        Description = "魔法詠唱の成功率を上げる",
        Name = "詠唱",
    },
    ControlMagic = {
        Description = "魔法による仲間のまきこみを軽減する",
        Name = "魔力制御",
    },
    Cooking = {
        Description = "料理の腕を上げる",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s料理の腕を上げる"):format(_.kare_wa(item))
        end,
        Name = "料理",
    },
    Crossbow = {
        Description = "クロスボウを扱う技術",
        Name = "クロスボウ",
        Damage = {
            WeaponName = "クロスボウ",
            VerbPassive = "撃たれた",
            VerbActive = "射撃し",
        },
    },
    Detection = {
        Description = "隠された場所や罠を見つける",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s探知能力を強化する"):format(_.kare_wa(item))
        end,
        Name = "探知",
    },
    DisarmTrap = {
        Description = "複雑な罠の解体を可能にする",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s罠の解体を容易にする"):format(_.kare_wa(item))
        end,
        Name = "罠解体",
    },
    DualWield = {
        Description = "複数の武器を扱う技術",
        Name = "二刀流",
    },
    Evasion = {
        Description = "攻撃を回避する",
        Name = "回避",
    },
    EyeOfMind = {
        Description = "クリティカル率を高める",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s心眼の技術を上昇させる"):format(_.kare_wa(item))
        end,
        Name = "心眼",
    },
    Faith = {
        Description = "神との距離を近める",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s信仰を深める"):format(_.kare_wa(item))
        end,
        Name = "信仰",
    },
    Firearm = {
        Description = "遠隔機装を扱う技術",
        Name = "銃器",
        Damage = {
            WeaponName = "銃",
            VerbPassive = "撃たれた",
            VerbActive = "射撃し",
        },
    },
    Fishing = {
        Description = "釣りを可能にする",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s釣りの腕を上げる"):format(_.kare_wa(item))
        end,
        Name = "釣り",
    },
    Gardening = {
        Description = "植物を育て、採取する",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s栽培の腕を上げる"):format(_.kare_wa(item))
        end,
        Name = "栽培",
    },
    GeneEngineer = {
        Description = "仲間合成の知識を高める",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s遺伝子学の知識を深める"):format(_.kare_wa(item))
        end,
        Name = "遺伝子学",
    },
    GreaterEvasion = {
        Description = "不正確な攻撃を確実に避ける",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s見切りの腕を上げる"):format(_.kare_wa(item))
        end,
        Name = "見切り",
    },
    Healing = {
        Description = "怪我を自然に治癒する",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s体力回復を強化する"):format(_.kare_wa(item))
        end,
        Name = "治癒",
    },
    HeavyArmor = {
        Description = "重い装備を扱う技術",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s重装備の技術を上昇させる"):format(_.kare_wa(item))
        end,
        Name = "重装備",
    },
    Investing = {
        Description = "効果的に投資を行う",
        Name = "投資",
    },
    Jeweler = {
        Description = "宝石を加工し、アイテムを作り出す",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s宝石細工の腕を上げる"):format(_.kare_wa(item))
        end,
        Name = "宝石細工",
    },
    LightArmor = {
        Description = "軽い装備を扱う技術",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s軽装備の技術を上昇させる"):format(_.kare_wa(item))
        end,
        Name = "軽装備",
    },
    Literacy = {
        Description = "難解な本の解読を可能にする",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s本の理解を深める"):format(_.kare_wa(item))
        end,
        Name = "読書",
    },
    LockPicking = {
        Description = "鍵を開ける",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s鍵開けの能力を強化する"):format(_.kare_wa(item))
        end,
        Name = "鍵開け",
    },
    LongSword = {
        Description = "刃渡りの長い剣を扱う技術",
        Name = "長剣",
        Damage = {
            WeaponName = "長剣",
            VerbPassive = "切られた",
            VerbActive = "切り払い",
        },
    },
    MagicCapacity = {
        Description = "マナの反動から身を守る",
        EnchantmentDescription = function(item, wielder, power)
            return ("%sマナの限界を上昇させる"):format(_.kare_wa(item))
        end,
        Name = "魔力の限界",
    },
    MagicDevice = {
        Description = "道具から魔力を効果的に引き出す",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s魔道具の効果を上げる"):format(_.kare_wa(item))
        end,
        Name = "魔道具",
    },
    Marksman = {
        Description = "射撃の威力を上げる",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s射撃の理解を深める"):format(_.kare_wa(item))
        end,
        Name = "射撃",
    },
    MartialArts = {
        Description = "格闘の技術",
        Name = "格闘",
    },
    Meditation = {
        Description = "消耗したマナを回復させる",
        EnchantmentDescription = function(item, wielder, power)
            return ("%sマナ回復を強化する"):format(_.kare_wa(item))
        end,
        Name = "瞑想",
    },
    MediumArmor = {
        Description = "普通の装備を扱う技術",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s中装備の技術を上昇させる"):format(_.kare_wa(item))
        end,
        Name = "中装備",
    },
    Memorization = {
        Description = "書物から得た知識を記憶する",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s魔法の知識の忘却を防ぐ"):format(_.kare_wa(item))
        end,
        Name = "暗記",
    },
    Mining = {
        Description = "壁を掘る効率を上げる",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s採掘能力を強化する"):format(_.kare_wa(item))
        end,
        Name = "採掘",
    },
    Negotiation = {
        Description = "交渉や商談を有利に進める",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s交渉を有利に進めさせる"):format(_.kare_wa(item))
        end,
        Name = "交渉",
    },
    Performer = {
        Description = "質の高い演奏を可能にする",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s演奏の質を上げる"):format(_.kare_wa(item))
        end,
        Name = "演奏",
    },
    Pickpocket = {
        Description = "貴重な物品を盗む",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s窃盗の腕を上げる"):format(_.kare_wa(item))
        end,
        Name = "窃盗",
    },
    Polearm = {
        Description = "槍を扱う技術",
        Name = "槍",
        Damage = {
            WeaponName = "槍",
            VerbPassive = "刺された",
            VerbActive = "突き刺して",
        },
    },
    Riding = {
        Description = "上手に乗りこなす",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s乗馬の腕を上げる"):format(_.kare_wa(item))
        end,
        Name = "乗馬",
    },
    Scythe = {
        Description = "鎌を扱う技術",
        Name = "鎌",
        Damage = {
            WeaponName = "鎌",
            VerbPassive = "切られた",
            VerbActive = "切り払い",
        },
    },
    SenseQuality = {
        Description = "アイテムの質や種類を感じ取る",
        Name = "自然鑑定",
    },
    Shield = {
        Description = "盾を扱う技術",
        Name = "盾",
    },
    ShortSword = {
        Description = "刃渡りの短い剣を扱う技術",
        Name = "短剣",
        Damage = {
            WeaponName = "短剣",
            VerbPassive = "刺された",
            VerbActive = "突き刺して",
        },
    },
    Stave = {
        Description = "杖を扱う技術",
        Name = "杖",
        Damage = {
            WeaponName = "杖",
            VerbPassive = "打たれた",
            VerbActive = "打って",
        },
    },
    Stealth = {
        Description = "周囲に気づかれず行動する",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s隠密能力を強化する"):format(_.kare_wa(item))
        end,
        Name = "隠密",
    },
    Tactics = {
        Description = "近接攻撃の威力を上げる",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s戦術の理解を深める"):format(_.kare_wa(item))
        end,
        Name = "戦術",
    },
    Tailoring = {
        Description = "革や蔓を用い、アイテムを作り出す",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s裁縫の腕を上げる"):format(_.kare_wa(item))
        end,
        Name = "裁縫",
    },
    Throwing = {
        Description = "投擲道具を扱う技術",
        Name = "投擲",
        Damage = {
            WeaponName = "飛び道具",
            VerbPassive = "攻撃された",
            VerbActive = "投げ",
            AttacksActive = function(attacker, verb, target, weapon)
                return ("%s%sに%sを%s"):format(_.kare_wa(attacker), _.name(target), _.name(weapon), verb)
            end,
        },
    },
    Traveling = {
        Description = "旅の進行を早め経験を深める",
        EnchantmentDescription = function(item, wielder, power)
            return ("%s旅の熟練を上げる"):format(_.kare_wa(item))
        end,
        Name = "旅歩き",
    },
    TwoHand = {
        Description = "両手で武器を扱う技術",
        Name = "両手持ち",
    },
    WeightLifting = {
        Description = "重い荷物を持ち運ぶことを可能にする",
        Name = "重量挙げ",
    },
}
