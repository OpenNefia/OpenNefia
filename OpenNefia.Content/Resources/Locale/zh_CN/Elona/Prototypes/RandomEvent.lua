OpenNefia.Prototypes.Elona.RandomEvent.Elona = {
AvoidingMisfortune = {
    Title = "避免厄运",
    Text = "你曾经感到不好的预感，但它很快消失了。",
    Choices = {
        ["0"] = "好的",
    },
},
CampingSite = {
    Title = "发现野营地遗迹",
    Text = "你发现有人在这里设立过野营地。周围散落着剩菜剩饭和杂物。也许有些东西可能会有用。",
    Choices = {
        ["0"] = "调查",
        ["1"] = "离开",
    },
},
Corpse = {
    Title = "冒险者的尸体",
    Text = "你发现了一名冒险者的尸体，他在这个地方力竭而亡。散落着已经开始腐烂的骨头和一些装备。",
    Choices = {
        ["0"] = "搜寻",
        ["1"] = "埋葬",
    },

    Loot = "你搜寻了遗留物品。",
    Bury = "你埋葬了骨头和遗留物品。",
},
SmallLuck = {
    Title = "材料的发现",
    Text = "你被一个小石头绊倒，摔倒的时候，发现了一些材料。",
    Choices = {
        ["0"] = "好的",
    },
},
SmellOfFood = {
    Title = "美食的香味",
    Text = "一股美食的香味飘来，你的胃开始抗议了。",
    Choices = {
        ["0"] = "肚子饿了……",
    },
},
StrangeFeast = {
    Title = "神秘的盛宴",
    Text = "你在面前发现了一桌美食。",
    Choices = {
        ["0"] = "吃",
        ["1"] = "离开",
    },
},
Murderer = {
    Title = "杀人犯",
    Text = "城市中的某个地方传来了惊叫声。你目睹了卫兵们紧张地奔跑。“凶手！凶手！”",
    Choices = {
        ["0"] = "我念经中……",
    },

    Scream = function(victim)
        return ("%s“啊啊啊！”"):format(_.name(victim))
    end,
},
MadMillionaire = {
    Title = "发疯的富翁",
    Text = "一个发疯的富翁大声尖叫着扔掉金币……",
    Choices = {
        ["0"] = "太幸运了！",
    },

    YouPickUp = function(amount)
        return ("你获得了%s枚金币。"):format(amount)
    end,
},WanderingPriest = {
        Title = "流浪的神父",
        Text = "突然，一位神父从对面过来，趁你路过时向你施放了一个魔法。「没问题」",
        Choices = {
            ["0"] = "谢谢",
        },
    },
    GainingFaith = {
        Title = "信仰的加深",
        Text = "在梦中，你感受到伟大存在的慈祥威光。",
        Choices = {
            ["0"] = "神啊",
        },
    },
    TreasureOfDream = {
        Title = "埋藏宝藏的梦",
        Text = "你在梦中埋藏了一份宝藏。你立刻醒了过来，将那个地点记在了纸上。",
        Choices = {
            ["0"] = "哇哦",
        },
    },
    WizardsDream = {
        Title = "魔法师的梦",
        Text = "在梦中，你遇到了一位红发魔法师。「你又是谁？哦，看来出现在错误的梦境中了。抱歉。虽然不知道该怎么道歉…」魔法师转动手指。你感到头痛稍微缓解了些。",
        Choices = {
            ["0"] = "真是奇怪的梦",
        },
    },
    LuckyDay = {
        Title = "幸运之日",
        Text = "乌米咪呀！",
        Choices = {
            ["0"] = "哇哦",
        },
    },
    QuirkOfFate = {
        Title = "命运的变幻",
        Text = "乌米咪呀，找到了吧！",
        Choices = {
            ["0"] = "哇哦",
        },
    },
    MonsterDream = {
        Title = "怪物的梦",
        Text = "你和怪物战斗。当你试图刺杀那个丑陋的怪物时，它发出一声尖叫。「我就是你！你就是我！」你被自己的呻吟声吵醒了。",
        Choices = {
            ["0"] = "嗯…",
        },
    },
    DreamHarvest = {
        Title = "梦中的收获",
        Text = "在梦中，你悠闲地采集材料。",
        Choices = {
            ["0"] = "轻快♪",
        },
    },YourPotential = {
        Title = "才能的开花",
        Text = "突然你的才能开花了！",
        Choices = {
            ["0"] = "哇哦",
        },
    },
    Development = {
        Title = "成长的迹象",
        Text = "经过多年的磨炼，你的努力终于有了回报。在难以入眠且思考着事情的时候，你突然想出了关于自己技术的新主意。",
        Choices = {
            ["0"] = "好的！",
        },
    },
    CreepyDream = {
        Title = "令人毛骨悚然的梦境",
        Text = "你做了一个令人毛骨悚然的梦。一双阴森的眼睛盯着你，从无处传来笑声。“咯咯咯...找到你了...咯咯咯。”你翻了个身后，梦境就结束了。",
        Choices = {
            ["0"] = "好奇怪的梦",
        },
    },
    CursedWhispering = {
        Title = "诅咒的低语",
        Text = "一声声诅咒低语从无处传来，打扰了你的睡眠。",
        Choices = {
            ["0"] = "不能入睡...",
        },

        NoEffect = "你祈祷了一番，使诅咒低语无效了。",
        Beggars = "盗贼盯上了你！",
    },
    Regeneration = {
        Title = "自然疗愈力的提升",
        Text = "你醒来时身体异常发热。突然间，你注意到之前的伤痕全部消失了。",
        Choices = {
            ["0"] = "好的",
        },
    },
    Meditation = {
        Title = "冥想力的提升",
        Text = "你在梦境中保持了惊人的理智。如同在进行冥想一样，你感受到内心的平静。",
        Choices = {
            ["0"] = "好的",
        },
    },
    MaliciousHand = {
        Title = "恶意之手",
        Text = "一只恶意之手悄悄接近，趁你不注意偷走了金币后逃走了。",
        Choices = {
            ["0"] = "可恶的小偷...",
        },

        YouLose = function(amount)
            return ("你失去了%s枚金币。"):format(amount)
        end,
        NoEffect = "没有受到伤害。",
    },
    GreatLuck = {
        Title = "街头的好运",
        Text = "当你低着头走路时，幸运地发现了一枚白银硬币。",
        Choices = {
            ["0"] = "幸运！",
        },
    },Marriage = {
    Title = "结婚",
    Text = function(lover)
        return (
            "经过漫长的交往，你和%s终于坚定地走到了一起。在婚礼结束后，你收到了一些礼物。"
        ):format(_.name(lover))
    end,
    Choices = {
        ["0"] = "将一生献给你",
    },
},
ReunionWithPet = {
    Title = "与宠物重聚",
    Text = "你突然听到了熟悉的叫声，停下脚步。你的宠物竟然欢快地奔跑过来，它曾在船难中失去。你的宠物是...",
    Choices = {
        ["0"] = "狗！",
        ["1"] = "猫！",
        ["2"] = "熊！",
        ["3"] = "少女！",
    },
},
}