Elona.Wishes = {
    Prompt = function(wisher)
        return ("%s，你想要什么？"):format(_.sore_wa(wisher))
    end,
    YouWish = function(wisher, wish)
        return _.quote(("%s！！"):format(wish))
    end,

    ItIsSoldOut = "啊，这个已经卖完了。",

    SomethingAppears = {
        Normal = function(wisher, item)
            return ("一样%s滚到了你的脚边。"):format(_.name(item))
        end,
        FromNowhere = function(wisher, item)
            return ("一样%s从无处滚到了你的脚边。"):format(_.name(item))
        end,
        FallsDown = function(wisher, item)
            return ("%s从天上掉落！"):format(_.name(item))
        end,
    },

    General = {
        Card = {
            Keyword = "卡片",
        },
        Figure = {
            Keyword = { "剥制", "剥製" },
        },
        Item = {
            Keyword = "物品",
        },
        Skill = {
            Keyword = "技能",

            Gain = function(wisher, skillName)
                return ("获得%s的技能！"):format(skillName)
            end,
            Improve = function(wisher, skillName)
                return ("%s提升了！"):format(skillName)
            end,
        },
        Summon = {
            Keyword = "召唤",
        },
    },
}