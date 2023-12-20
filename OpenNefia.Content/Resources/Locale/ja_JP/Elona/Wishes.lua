Elona.Wishes = {
    Prompt = function(wisher)
        return ("%s何を望む？"):format(_.sore_wa(wisher))
    end,
    YouWish = function(wisher, wish)
        return _.quote(("%s！！"):format(wish))
    end,

    ItIsSoldOut = "あ、それ在庫切れ。",

    SomethingAppears = {
        Normal = function(wisher, item)
            return ("足元に%sが転がってきた。"):format(_.name(item))
        end,
        FromNowhere = function(wisher, item)
            return ("足元に%sが転がってきた。"):format(_.name(item))
        end,
        FallsDown = function(wisher, item)
            ("%sが降ってきた！"):format(_.name(item))
        end,
    },

    General = {
        Card = {
            Keyword = "カード",
        },
        Figure = {
            Keyword = { "剥製", "はく製" },
        },
        Item = {
            Keyword = "アイテム",
        },
        Skill = {
            Keyword = "スキル",

            Gain = function(wisher, skillName)
                return ("%sの技術を会得した！"):format(skillName)
            end,
            Improve = function(wisher, skillName)
                return ("%sが上昇した！"):format(skillName)
            end,
        },
        Summon = {
            Keyword = "召喚",
        },
    },
}
