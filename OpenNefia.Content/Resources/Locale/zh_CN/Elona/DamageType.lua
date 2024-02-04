Elona.DamageType = {
    Default = {
        Damage = function(entity)
            return ("%s受伤了。"):format(_.name(entity))
        end,
        Death = {
            Active = "击杀了。",
            Passive = function(entity)
                return ("%s死亡了。"):format(_.name(entity))
            end,
        },
    },
    Combat = {
        DeathCause = function(entity)
            return ("%s被%s杀死了。"):format(_.basename(entity))
        end,
        Killed = {
            Active = "击杀了。",
            Passive = function(entity)
                return ("%s被杀死了。"):format(_.name(entity))
            end,
        },
        Minced = {
            Active = "制成肉酱。",
            Passive = function(entity)
                return ("%s被制成肉酱。"):format(_.name(entity))
            end,
        },
        TransformedIntoMeat = {
            Active = "被转化为肉块。",
            Passive = function(entity)
                return ("%s被转化为肉块。"):format(_.name(entity))
            end,
        },
        Destroyed = {
            Active = "摧毁了。",
            Passive = function(entity)
                return ("%s被摧毁了。"):format(_.name(entity))
            end,
        },
    },
    Trap = {
        DeathCause = "被陷阱杀死了。",
        Message = function(entity)
            return ("%s被陷阱杀死了。"):format(_.name(entity))
        end,
    },
    MagicReaction = {
        DeathCause = "因魔力反应消失了。",
        Message = function(entity)
            return ("%s因魔力反应死亡了。"):format(_.name(entity))
        end,
    },
    Starvation = {
        DeathCause = "饿死了。",
        Message = function(entity)
            return ("%s因饥饿而死亡。"):format(_.name(entity))
        end,
    },
    Poison = {
        DeathCause = "中毒而死。",
        Message = function(entity)
            return ("%s被毒素侵蚀而死亡。"):format(_.name(entity))
        end,
    },
    Curse = {
        DeathCause = "被诅咒杀死了。",
        Message = function(entity)
            return ("%s因诅咒之力而死亡。"):format(_.name(entity))
        end,
    },
    Burden = {
        Backpack = "背包",
        DeathCause = function(itemName)
            return ("%s无法承受重量而崩溃。"):format(itemName)
        end,
        Message = function(entity, item)
            return ("%s无法承受%s的重量而死亡。"):format(_.name(entity), _.name(item))
        end,
    },
    Stairs = {
        DeathCause = "从楼梯上摔下来而去世了。",
        Message = function(entity)
            return ("%s从楼梯上摔下来而死亡。"):format(_.name(entity))
        end,
    },
    Performance = {
        DeathCause = "被愤怒的听众杀死了。",
        Message = function(entity)
            return ("%s被听众杀死了。"):format(_.name(entity))
        end,
    },
    Burning = {
        DeathCause = "被烧成灰烬了。",
        Message = function(entity)
            return ("%s被烧死了。"):format(_.name(entity))
        end,
    },
    UnseenHand = {
        DeathCause = "被无形之手埋葬了。",
        Message = function(entity)
            return ("%s被无形之手杀死了。"):format(_.name(entity))
        end,
    },
    FoodPoisoning = {
        DeathCause = "食物中毒而倒下。",
        Message = function(entity)
            return ("%s因食物中毒而死亡。"):format(_.name(entity))
        end,
    },
    Bleeding = {
        DeathCause = "失血过多而死亡。",
        Message = function(entity)
            return ("%s因大量出血而死亡。"):format(_.name(entity))
        end,
    },
    EtherDisease = {
        DeathCause = "被以太侵蚀而倒下。",
        Message = function(entity)
            return ("%s因被以太侵蚀而死亡。"):format(_.name(entity))
        end,
    },
    Acid = {
        DeathCause = "被溶解成液体了。",
        Message = function(entity)
            return ("%s被溶解成液体了。"):format(_.name(entity))
        end,
    },
    Suicide = {
        DeathCause = "自杀了。",
        Message = function(entity)
            return ("%s被撕成碎片了。"):format(_.name(entity))
        end,
    },
    Nuke = {
        DeathCause = "被核爆炸卷入而死亡。",
        Message = function(entity)
            return ("%s被核爆炸卷入而变成了灰烬。"):format(_.name(entity))
        end,
    },
    IronMaiden = {
        DeathCause = "被铁处女活活夹死了。",
        Message = function(entity)
            return ("%s在铁处女中串刺而倒下了。"):format(_.name(entity))
        end,
    },
    Guillotine = {
        DeathCause = "被断头台砍下了头而死亡。",
        Message = function(entity)
            return ("%s被断头台砍下了头而死亡。"):format(_.name(entity))
        end,
    },
    Hanging = {
        DeathCause = "上吊自尽了。",
        Message = function(entity)
            return ("%s上吊自尽了。"):format(_.name(entity))
        end,
    },
    Mochi = {
        DeathCause = "被餅黏住喉咙而死亡。",
        Message = function(entity)
            return ("%s被餅黏住喉咙而死亡。"):format(_.name(entity))
        end,
    },
}