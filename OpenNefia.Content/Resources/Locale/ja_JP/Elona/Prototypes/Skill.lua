OpenNefia.Prototypes.Elona.Skill.Elona = {
    StatStrength = {
        Name = "筋力",
        ShortName = "筋力",

        OnDecrease = function(entity)
            return ("%sは少し贅肉が増えたような気がした。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sはより強くなった。"):format(_.name(entity))
        end,
    },
    StatConstitution = {
        Name = "耐久",
        ShortName = "耐久",

        OnDecrease = function(entity)
            return ("%sは我慢ができなくなった。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは我慢することの快感を知った。"):format(_.name(entity))
        end,
    },
    StatDexterity = {
        Name = "器用",
        ShortName = "器用",

        OnDecrease = function(entity)
            return ("%sは不器用になった。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは器用になった。"):format(_.name(entity))
        end,
    },
    StatPerception = {
        Name = "感覚",
        ShortName = "感覚",

        OnDecrease = function(entity)
            return ("%sは感覚のずれを感じた。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは世界をより身近に感じるようになった。"):format(_.name(entity))
        end,
    },
    StatLearning = {
        Name = "習得",
        ShortName = "習得",

        OnDecrease = function(entity)
            return ("%sの学習意欲が低下した。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは急に色々なことを学びたくなった。"):format(_.name(entity))
        end,
    },
    StatWill = {
        Name = "意思",
        ShortName = "意思",

        OnDecrease = function(entity)
            return ("%sは何でもすぐ諦める。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sの意思は固くなった。"):format(_.name(entity))
        end,
    },
    StatMagic = {
        Name = "魔力",
        ShortName = "魔力",

        OnDecrease = function(entity)
            return ("%sは魔力の衰えを感じた。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは魔力の上昇を感じた。"):format(_.name(entity))
        end,
    },
    StatCharisma = {
        Name = "魅力",
        ShortName = "魅力",

        OnDecrease = function(entity)
            return ("%sは急に人前に出るのが嫌になった。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは周囲の視線を心地よく感じる。"):format(_.name(entity))
        end,
    },
    StatSpeed = {
        Name = "速度",

        OnDecrease = function(entity)
            return ("%sは遅くなった。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは周りの動きが遅く見えるようになった。"):format(_.name(entity))
        end,
    },
    StatLuck = {
        Name = "運勢",

        OnDecrease = function(entity)
            return ("%sは不幸になった。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは幸運になった。"):format(_.name(entity))
        end,
    },
    StatLife = {
        Name = "生命力",

        OnDecrease = function(entity)
            return ("%sは生命力の衰えを感じた。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sは生命力の上昇を感じた。"):format(_.name(entity))
        end,
    },
    StatMana = {
        Name = "マナ",

        OnDecrease = function(entity)
            return ("%sはマナの衰えを感じた。"):format(_.name(entity))
        end,
        OnIncrease = function(entity)
            return ("%sはマナの向上を感じた。"):format(_.name(entity))
        end,
    },

    ActionAbsorbMagic = {
        Description = "マナ回復",
        Name = "魔力の吸収",
    },
    SpellAcidGround = {
        Description = "酸の発生",
        Name = "酸の海",
    },
    Alchemy = {
        Description = "様々な材料を調合し、ポーションを作り出す",
        EnchantmentDescription = "錬金の腕を上げる",
        Name = "錬金術",
    },
    Anatomy = {
        Description = "死体を残しやすくする",
        EnchantmentDescription = "死体を残しやすくする",
        Name = "解剖学",
    },
    Axe = {
        Description = "斧を扱う技術",
        Name = "斧",
    },
    Blunt = {
        Description = "鈍器を扱う技術",
        Name = "鈍器",
    },
    BuffBoost = {
        Name = "ブースト",
    },
    Bow = {
        Description = "弓を扱う技術",
        Name = "弓",
    },
    Carpentry = {
        Description = "木を加工し、アイテムを作り出す",
        EnchantmentDescription = "大工の腕を上げる",
        Name = "大工",
    },
    Casting = {
        Description = "魔法詠唱の成功率を上げる",
        Name = "詠唱",
    },
    ActionChange = {
        Description = "対象変容",
        Name = "他者変容",
    },
    SpellChaosBall = {
        Description = "混沌の球",
        Name = "混沌の渦",
    },
    ActionChaosBreath = {
        Description = "混沌のブレス",
        Name = "混沌のブレス",
    },
    SpellChaosEye = {
        Description = "混沌の矢",
        Name = "混沌の瞳",
    },
    ActionCheer = {
        Description = "視界内仲間強化",
        Name = "鼓舞",
    },
    ActionColdBreath = {
        Description = "冷気のブレス",
        Name = "冷気のブレス",
    },
    BuffContingency = {
        Name = "契約",
    },
    ControlMagic = {
        Description = "魔法による仲間のまきこみを軽減する",
        Name = "魔力制御",
    },
    Cooking = {
        Description = "料理の腕を上げる",
        EnchantmentDescription = "料理の腕を上げる",
        Name = "料理",
    },
    Crossbow = {
        Description = "クロスボウを扱う技術",
        Name = "クロスボウ",
    },
    SpellCrystalSpear = {
        Description = "無属性の矢",
        Name = "魔力の集積",
    },
    SpellCureOfEris = {
        Description = "体力回復",
        Name = "エリスの癒し",
    },
    SpellCureOfJure = {
        Description = "体力回復",
        Name = "ジュアの癒し",
    },
    ActionCurse = {
        Description = "呪いをかける",
        Name = "呪いの言葉",
    },
    SpellDarkEye = {
        Description = "暗黒の矢",
        Name = "暗黒の矢",
    },
    SpellDarknessBolt = {
        Description = "暗黒のボルト",
        Name = "暗黒の光線",
    },
    ActionDarknessBreath = {
        Description = "暗黒のブレス",
        Name = "暗黒のブレス",
    },
    BuffDeathWord = {
        Name = "死の宣告",
    },
    ActionDecapitation = {
        Description = "対象即死",
        Name = "首狩り",
    },
    Detection = {
        Description = "隠された場所や罠を見つける",
        EnchantmentDescription = "探知能力を強化する",
        Name = "探知",
    },
    ActionDimensionalMove = {
        Description = "近くへの瞬間移動",
        Name = "空間歪曲",
    },
    DisarmTrap = {
        Description = "複雑な罠の解体を可能にする",
        EnchantmentDescription = "罠の解体を容易にする",
        Name = "罠解体",
    },
    ActionDistantAttack4 = {
        Name = "遠距離打撃",
    },
    ActionDistantAttack7 = {
        Name = "遠距離打撃",
    },
    BuffDivineWisdom = {
        Name = "知者の加護",
    },
    SpellDominate = {
        Description = "対象を支配する",
        Name = "支配",
    },
    SpellDoorCreation = {
        Description = "ドアの生成",
        Name = "ドア生成",
    },
    ActionDrainBlood = {
        Description = "体力吸収",
        Name = "吸血の牙",
    },
    ActionDrawCharge = {
        Description = "杖から魔力抽出",
        Name = "魔力の抽出",
    },
    ActionDrawShadow = {
        Description = "対象をテレポート",
        Name = "異次元の手",
    },
    ActionDropMine = {
        Description = "足元に地雷設置",
        Name = "地雷投下",
    },
    DualWield = {
        Description = "複数の武器を扱う技術",
        Name = "二刀流",
    },
    BuffElementScar = {
        Name = "元素の傷跡",
    },
    BuffElementalShield = {
        Name = "元素保護",
    },
    ActionEtherGround = {
        Description = "エーテルの発生",
        Name = "エーテルの海",
    },
    Evasion = {
        Description = "攻撃を回避する",
        Name = "回避",
    },
    ActionEyeOfDimness = {
        Description = "対象朦朧",
        Name = "朦朧の眼差し",
    },
    ActionEyeOfEther = {
        Description = "対象エーテル侵食",
        Name = "エーテルの眼差し",
    },
    ActionEyeOfInsanity = {
        Description = "対象狂気",
        Name = "狂気の眼差し",
    },
    ActionEyeOfMana = {
        Description = "マナダメージ",
        Name = "マナの眼差し",
    },
    EyeOfMind = {
        Description = "クリティカル率を高める",
        EnchantmentDescription = "心眼の技術を上昇させる",
        Name = "心眼",
    },
    ActionEyeOfMutation = {
        Description = "対象変容",
        Name = "変容の眼差し",
    },
    Faith = {
        Description = "神との距離を近める",
        EnchantmentDescription = "信仰を深める",
        Name = "信仰",
    },
    ActionFillCharge = {
        Description = "充填",
        Name = "魔力の充填",
    },
    SpellFireBall = {
        Description = "炎の球",
        Name = "ファイアボール",
    },
    SpellFireBolt = {
        Description = "火炎のボルト",
        Name = "ファイアボルト",
    },
    ActionFireBreath = {
        Description = "炎のブレス",
        Name = "炎のブレス",
    },
    SpellFireWall = {
        Description = "火柱の発生",
        Name = "炎の壁",
    },
    Firearm = {
        Description = "遠隔機装を扱う技術",
        Name = "銃器",
    },
    Fishing = {
        Description = "釣りを可能にする",
        EnchantmentDescription = "釣りの腕を上げる",
        Name = "釣り",
    },
    SpellFourDimensionalPocket = {
        Description = "四次元のポケットを召喚",
        Name = "四次元ポケット",
    },
    Gardening = {
        Description = "植物を育て、採取する",
        EnchantmentDescription = "栽培の腕を上げる",
        Name = "栽培",
    },
    GeneEngineer = {
        Description = "仲間合成の知識を高める",
        EnchantmentDescription = "遺伝子学の知識を深める",
        Name = "遺伝子学",
    },
    SpellGravity = {
        Description = "重力の発生",
        Name = "グラビティ",
    },
    GreaterEvasion = {
        Description = "不正確な攻撃を確実に避ける",
        EnchantmentDescription = "見切りの腕を上げる",
        Name = "見切り",
    },
    ActionGrenade = {
        Description = "轟音の球",
        Name = "グレネード",
    },
    ActionHarvestMana = {
        Description = "マナ回復",
        Name = "マナ回復",
    },
    SpellHealCritical = {
        Description = "体力回復",
        Name = "致命傷治癒",
    },
    SpellHealLight = {
        Description = "体力回復",
        Name = "軽傷治癒",
    },
    Healing = {
        Description = "怪我を自然に治癒する",
        EnchantmentDescription = "体力回復を強化する",
        Name = "治癒",
    },
    SpellHealingRain = {
        Description = "体力回復の球",
        Name = "治癒の雨",
    },
    SpellHealingTouch = {
        Description = "体力回復",
        Name = "癒しの手",
    },
    HeavyArmor = {
        Description = "重い装備を扱う技術",
        EnchantmentDescription = "重装備の技術を上昇させる",
        Name = "重装備",
    },
    BuffHero = {
        Name = "英雄",
    },
    SpellHolyLight = {
        Description = "1つの呪い(hex)除去",
        Name = "清浄なる光",
    },
    BuffHolyShield = {
        Name = "聖なる盾",
    },
    BuffHolyVeil = {
        Name = "ホーリーヴェイル",
    },
    SpellIceBall = {
        Description = "氷の球",
        Name = "アイスボール",
    },
    SpellIceBolt = {
        Description = "氷のボルト",
        Name = "アイスボルト",
    },
    SpellIdentify = {
        Description = "アイテム鑑定",
        Name = "鑑定",
    },
    BuffIncognito = {
        Name = "インコグニート",
    },
    ActionInsult = {
        Description = "対象朦朧",
        Name = "罵倒",
    },
    Investing = {
        Description = "効果的に投資を行う",
        Name = "投資",
    },
    Jeweler = {
        Description = "宝石を加工し、アイテムを作り出す",
        EnchantmentDescription = "宝石細工の腕を上げる",
        Name = "宝石細工",
    },
    LightArmor = {
        Description = "軽い装備を扱う技術",
        EnchantmentDescription = "軽装備の技術を上昇させる",
        Name = "軽装備",
    },
    SpellLightningBolt = {
        Description = "雷のボルト",
        Name = "ライトニングボルト",
    },
    ActionLightningBreath = {
        Description = "電撃のブレス",
        Name = "電撃のブレス",
    },
    Literacy = {
        Description = "難解な本の解読を可能にする",
        EnchantmentDescription = "本の理解を深める",
        Name = "読書",
    },
    LockPicking = {
        Description = "鍵を開ける",
        EnchantmentDescription = "鍵開けの能力を強化する",
        Name = "鍵開け",
    },
    LongSword = {
        Description = "刃渡りの長い剣を扱う技術",
        Name = "長剣",
    },
    BuffLulwysTrick = {
        Name = "ルルウィの憑依",
    },
    MagicCapacity = {
        Description = "マナの反動から身を守る",
        EnchantmentDescription = "マナの限界を上昇させる",
        Name = "魔力の限界",
    },
    SpellMagicDart = {
        Description = "無属性の矢",
        Name = "魔法の矢",
    },
    MagicDevice = {
        Description = "道具から魔力を効果的に引き出す",
        EnchantmentDescription = "魔道具の効果を上げる",
        Name = "魔道具",
    },
    SpellMagicMap = {
        Description = "周囲の地形感知",
        Name = "魔法の地図",
    },
    SpellMagicStorm = {
        Description = "魔法の球",
        Name = "魔力の嵐",
    },
    ActionManisDisassembly = {
        Description = "敵瀕死",
        Name = "マニの分解術",
    },
    Marksman = {
        Description = "射撃の威力を上げる",
        EnchantmentDescription = "射撃の理解を深める",
        Name = "射撃",
    },
    MartialArts = {
        Description = "格闘の技術",
        Name = "格闘",
    },
    Meditation = {
        Description = "消耗したマナを回復させる",
        EnchantmentDescription = "マナ回復を強化する",
        Name = "瞑想",
    },
    MediumArmor = {
        Description = "普通の装備を扱う技術",
        EnchantmentDescription = "中装備の技術を上昇させる",
        Name = "中装備",
    },
    Memorization = {
        Description = "書物から得た知識を記憶する",
        EnchantmentDescription = "魔法の知識の忘却を防ぐ",
        Name = "暗記",
    },
    SpellMeteor = {
        Description = "全域攻撃",
        Name = "メテオ",
    },
    ActionMewmewmew = {
        Description = "？",
        Name = "うみみゃぁ！",
    },
    SpellMindBolt = {
        Description = "幻惑のボルト",
        Name = "幻影の光線",
    },
    ActionMindBreath = {
        Description = "幻惑のブレス",
        Name = "幻惑のブレス",
    },
    Mining = {
        Description = "壁を掘る効率を上げる",
        EnchantmentDescription = "採掘能力を強化する",
        Name = "採掘",
    },
    ActionMirror = {
        Description = "自分の状態の感知",
        Name = "自己認識",
    },
    SpellMistOfDarkness = {
        Description = "霧の発生",
        Name = "闇の霧",
    },
    BuffMistOfFrailness = {
        Name = "脆弱の霧",
    },
    BuffMistOfSilence = {
        Name = "沈黙の霧",
    },
    SpellMutation = {
        Description = "突然変異",
        Name = "自己の変容",
    },
    Negotiation = {
        Description = "交渉や商談を有利に進める",
        EnchantmentDescription = "交渉を有利に進めさせる",
        Name = "交渉",
    },
    SpellNerveArrow = {
        Description = "神経の矢",
        Name = "麻痺の矢",
    },
    ActionNerveBreath = {
        Description = "神経のブレス",
        Name = "神経のブレス",
    },
    SpellNetherArrow = {
        Description = "地獄の矢",
        Name = "地獄の吐息",
    },
    ActionNetherBreath = {
        Description = "地獄のブレス",
        Name = "地獄のブレス",
    },
    BuffNightmare = {
        Name = "ナイトメア",
    },
    SpellOracle = {
        Description = "アーティファクト感知",
        Name = "神託",
    },
    Performer = {
        Description = "質の高い演奏を可能にする",
        EnchantmentDescription = "演奏の質を上げる",
        Name = "演奏",
    },
    Pickpocket = {
        Description = "貴重な物品を盗む",
        EnchantmentDescription = "窃盗の腕を上げる",
        Name = "窃盗",
    },
    ActionPoisonBreath = {
        Description = "毒のブレス",
        Name = "毒のブレス",
    },
    Polearm = {
        Description = "槍を扱う技術",
        Name = "槍",
    },
    ActionPowerBreath = {
        Description = "ブレス",
        Name = "強力なブレス",
    },
    ActionPrayerOfJure = {
        Description = "体力回復",
        Name = "ジュアの祈り",
    },
    ActionPregnant = {
        Description = "対象妊娠",
        Name = "妊娠",
    },
    BuffPunishment = {
        Name = "神罰",
    },
    SpellRagingRoar = {
        Description = "轟音の球",
        Name = "轟音の波動",
    },
    ActionRainOfSanity = {
        Description = "狂気回復の球",
        Name = "狂気治癒の雨",
    },
    BuffRegeneration = {
        Name = "リジェネレーション",
    },
    SpellRestoreBody = {
        Description = "肉体の弱体化の治療",
        Name = "肉体復活",
    },
    SpellRestoreSpirit = {
        Description = "精神の弱体化の治療",
        Name = "精神復活",
    },
    SpellResurrection = {
        Description = "死者の蘇生",
        Name = "復活",
    },
    SpellReturn = {
        Description = "特定の場所への帰還",
        Name = "帰還",
    },
    Riding = {
        Description = "上手に乗りこなす",
        EnchantmentDescription = "乗馬の腕を上げる",
        Name = "乗馬",
    },
    ActionScavenge = {
        Description = "盗んで食べる",
        Name = "食い漁り",
    },
    Scythe = {
        Description = "鎌を扱う技術",
        Name = "鎌",
    },
    SpellSenseObject = {
        Description = "周囲の物質感知",
        Name = "物質感知",
    },
    SenseQuality = {
        Description = "アイテムの質や種類を感じ取る",
        Name = "自然鑑定",
    },
    ActionShadowStep = {
        Description = "対象へのテレポート",
        Name = "接近",
    },
    Shield = {
        Description = "盾を扱う技術",
        Name = "盾",
    },
    ShortSword = {
        Description = "刃渡りの短い剣を扱う技術",
        Name = "短剣",
    },
    SpellShortTeleport = {
        Description = "近くへの瞬間移動",
        Name = "ショートテレポート",
    },
    BuffSlow = {
        Name = "鈍足",
    },
    ActionSoundBreath = {
        Description = "轟音のブレス",
        Name = "轟音のブレス",
    },
    BuffSpeed = {
        Name = "加速",
    },
    Stave = {
        Description = "杖を扱う技術",
        Name = "杖",
    },
    Stealth = {
        Description = "周囲に気づかれず行動する",
        EnchantmentDescription = "隠密能力を強化する",
        Name = "隠密",
    },
    ActionSuicideAttack = {
        Description = "自爆の球",
        Name = "自爆",
    },
    ActionSummonCats = {
        Description = "猫を召喚する",
        Name = "猫召喚",
    },
    ActionSummonFire = {
        Description = "炎の生き物を召喚する",
        Name = "炎召喚",
    },
    SpellSummonMonsters = {
        Description = "モンスターを召喚する",
        Name = "モンスター召喚",
    },
    ActionSummonPawn = {
        Description = "駒を召喚する",
        Name = "駒召喚",
    },
    ActionSummonSister = {
        Description = "妹を召喚する",
        Name = "妹召喚",
    },
    SpellSummonWild = {
        Description = "野生の生き物を召喚する",
        Name = "野生召喚",
    },
    ActionSummonYeek = {
        Description = "イークを召喚する",
        Name = "イーク召喚",
    },
    ActionSuspiciousHand = {
        Description = "盗み",
        Name = "スリの指",
    },
    ActionSwarm = {
        Description = "隣接対象攻撃",
        Name = "スウォーム",
    },
    Tactics = {
        Description = "近接攻撃の威力を上げる",
        EnchantmentDescription = "戦術の理解を深める",
        Name = "戦術",
    },
    Tailoring = {
        Description = "革や蔓を用い、アイテムを作り出す",
        EnchantmentDescription = "裁縫の腕を上げる",
        Name = "裁縫",
    },
    SpellTeleport = {
        Description = "瞬間移動",
        Name = "テレポート",
    },
    SpellTeleportOther = {
        Description = "対象を瞬間移動させる",
        Name = "テレポートアザー",
    },
    Throwing = {
        Description = "投擲道具を扱う技術",
        Name = "投擲",
    },
    ActionTouchOfFear = {
        Description = "無属性攻撃",
        Name = "恐怖の手",
    },
    ActionTouchOfHunger = {
        Description = "飢餓攻撃",
        Name = "飢餓の手",
    },
    ActionTouchOfNerve = {
        Description = "神経攻撃",
        Name = "麻痺の手",
    },
    ActionTouchOfPoison = {
        Description = "毒攻撃",
        Name = "毒の手",
    },
    ActionTouchOfSleep = {
        Description = "精神攻撃",
        Name = "眠りの手",
    },
    ActionTouchOfWeakness = {
        Description = "弱体化",
        Name = "弱体化の手",
    },
    Traveling = {
        Description = "旅の進行を早め経験を深める",
        EnchantmentDescription = "旅の熟練を上げる",
        Name = "旅歩き",
    },
    TwoHand = {
        Description = "両手で武器を扱う技術",
        Name = "両手持ち",
    },
    SpellUncurse = {
        Description = "アイテム解呪",
        Name = "解呪",
    },
    ActionVanish = {
        Description = "退却する",
        Name = "退却",
    },
    SpellVanquishHex = {
        Description = "全ての呪い(hex)除去",
        Name = "全浄化",
    },
    SpellWallCreation = {
        Description = "壁の生成",
        Name = "壁生成",
    },
    SpellWeb = {
        Description = "蜘蛛の巣発生",
        Name = "蜘蛛の巣",
    },
    WeightLifting = {
        Description = "重い荷物を持ち運ぶことを可能にする",
        Name = "重量挙げ",
    },
    SpellWish = {
        Description = "願いの効果",
        Name = "願い",
    },
    SpellWizardsHarvest = {
        Description = "ランダムな収穫",
        Name = "魔術師の収穫",
    },
}
