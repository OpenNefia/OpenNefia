OpenNefia.Prototypes.Elona.StatusEffect.Elona = {
    Bleeding = {
        Apply = function(chara)
            return ("%s开始流血。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%s的出血停止了。"):format(_.name(chara))
        end,
        Indicator = {
            ["0"] = "切伤",
            ["1"] = "出血",
            ["2"] = "大出血",
        },
    },
    Blindness = {
        Apply = function(chara)
            return ("%s变得失明。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%s从失明中恢复了。"):format(_.name(chara))
        end,
        Blind = "失明",
    },
    Confusion = {
        Apply = function(chara)
            return ("%s变得混乱。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%s从混乱中恢复了。"):format(_.name(chara))
        end,
        Indicator = {
            ["0"] = "混乱",
        },
    },
    Dimming = {
        Apply = function(chara)
            return ("%s变得模糊。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%s的意识变得清晰了。"):format(_.name(chara))
        end,
        Indicator = {
            ["0"] = "模糊",
            ["1"] = "混浊",
            ["2"] = "昏迷",
        },
    },
    Drunk = {
        Apply = function(chara)
            return ("%s喝醉了。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%s醒了过来。"):format(_.name(chara))
        end,
        Indicator = {
            ["0"] = "醉酒",
            ["1"] = "醉酒",
        },
    },
    Fear = {
        Apply = function(chara)
            return ("%s受到恐惧侵袭。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%s从恐惧中恢复了。"):format(_.name(chara))
        end,
        Indicator = {
            ["0"] = "恐惧",
        },
    },
    Insanity = {
        Apply = function(chara)
            return ("%s发疯了。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%s恢复了正常。"):format(_.name(chara))
        end,
        Indicator = {
            ["0"] = "不稳定",
            ["1"] = "疯狂",
            ["2"] = "崩溃",
        },
        Dialog = {
            function(entity)
                return ("%s「咻咻咻」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「唧」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「夏亚啊啊」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「嘭嘭嘭！」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「煮死你！」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「请原谅，请原谅！！」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「呵哈哈！」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「啊、啊、啊、啊」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「噼…噼…噼噗…」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「哥哥！」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「乌微亚」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s突然开始跳舞。"):format(_.name(entity))
            end,
            function(entity)
                return ("%s开始脱衣服。"):format(_.name(entity))
            end,
            function(entity)
                return ("%s开始打转。"):format(_.name(entity))
            end,
            function(entity)
                return ("%s发出奇怪的声音。"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「妞~妞~♪妞~妞~♪」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「嗡滋穆♫嗡滋穆♫」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「那么只能杀了你了。嗯♪」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「这只蜗牛！」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「坐下！」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「呼呼呼呼… 呼呼呼呼… 呼呼呼呼」"):format(
                    _.name(entity)
                )
            end,
            function(entity)
                return ("%s「该死的蜗牛家伙！」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「乌尼咪咪！」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「对不起对不起！」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「马上要出生啦♪」"):format(_.name(entity))
            end,
            function(entity)
                return ("%s「呼… 呼… 啊…」"):format(_.name(entity))
            end,
        },
    },
    Paralysis = {
        Apply = function(chara)
            return ("%s被麻痹了。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%s从麻痹中恢复了。"):format(_.name(chara))
        end,
        Indicator = {
            ["0"] = "麻痹",
        },
    },
    Poison = {
        Apply = function(chara)
            return ("%s被毒药感染了。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%s从中毒中恢复了。"):format(_.name(chara))
        end,
        Indicator = {
            ["0"] = "中毒",
            ["1"] = "剧毒",
        },
    },
    Sick = {
        Apply = function(chara)
            return ("%s生病了。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%s病好了。"):format(_.name(chara))
        end,
        Indicator = {
            ["0"] = "生病",
            ["1"] = "重病",
        },
    },
    Sleep = {
        Apply = function(chara)
            return ("%s睡着了。"):format(_.name(chara))
        end,
        Heal = function(chara)
            return ("%s从美梦中醒来。"):format(_.name(chara))
        end,
        Indicator = {
            ["0"] = "睡眠",
            ["1"] = "熟睡",
        },
    },
    Choking = {
        Indicator = {
            ["0"] = "窒息",
        },
        Dialog = _.quote "呜咕咕…！",
    },
    Fury = {
        Indicator = {
            ["0"] = "激怒",
            ["1"] = "疯狂",
        },
    },
    Gravity = {
        Indicator = {
            ["0"] = "重力",
        },
    },
    Wet = {
        Indicator = {
            ["0"] = "湿润",
        },
    },
}