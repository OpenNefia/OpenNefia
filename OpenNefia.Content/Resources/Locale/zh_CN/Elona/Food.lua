Elona.Food = {
    ItemName = {
        Rotten = "腐った",
    },
    Cooking = {
        DoNotKnow = function(user)
            return ("%s不知道如何烹饪。"):format(_.sore_wa(user, 1))
        end,
        YouCook = function(user, oldFoodName, toolEntity, newFoodEntity)
            return ("%s用%s烹饪%s，制作出了%s。"):format(
                _.sore_wa(user, 1),
                _.name(toolEntity, 1),
                oldFoodName,
                _.name(newFoodEntity, 1)
            )
        end,
    },
    EatStatus = {
        Good = function(_1)
            return ("%s感觉很好。"):format(_.name(_1))
        end,
        Bad = function(_1)
            return ("%s感觉很不舒服。"):format(_.name(_1))
        end,
        CursedDrink = function(_1)
            return ("%s感觉非常不舒服。"):format(_.name(_1))
        end,
    },
    Nutrition = {
        Bloated = {
            "好久都不用再吃了。",
            "真的吃够了！",
            "实在是吃饱了！",
        },
        Satisfied = {
            "你感到满足。",
            "吃得太饱了！",
            "你的食欲得到了满足。",
            "你开心地抚摸了一下肚子。",
        },
        Normal = {
            "还可以再吃一点...",
            "你抚摸了一下肚子。",
            "稍微满足了一下食欲。",
        },
        Hungry = {
            "还没吃饱呢。",
            "有点不满足...",
            "还是有点饿。",
            "稍微填饱了一点肚子...",
        },
        VeryHungry = {
            "一点都没吃饱！",
            "连填饱肚子都不行。",
            "马上又饿了。",
        },
        Starving = {
            "这点食物没有意义！",
            "吃这点食物只是拖延了一下死亡。",
            "没有意义...需要摄取更多营养。",
        },
    },
    Message = {
        Quality = {
            Bad = { "啊...可能要拉肚子了。", "太难以下咽了！", "味道太糟糕了！" },
            SoSo = { "还不错的味道。", "味道还不错。" },
            Good = { "还挺好吃的。", "味道还可以。" },
            Great = { "很好吃！", "这个很不错！", "味道很棒！" },
            Delicious = { "非常好吃！", "天上的味道！", "绝品味道！" },
        },
        Uncooked = { "不是很糟糕，但也不太好吃...", "平凡的味道。" },
        Human = {
            Delicious = "好吃！",
            Dislike = "这是人肉...呕吐！",
            Like = "这是你最喜欢的人肉！",
            WouldHaveRatherEaten = "还是喜欢人肉...",
        },
        RawGlum = function(_1)
            return ("%s面露难色。"):format(_.name(_1))
        end,
        Rotten = "呃！吃了腐烂食物...呕吐...",
        Ability = {
            Deteriorates = function(_1, _2)
                return ("%s的%s能力下降了。"):format(_.name(_1), _2)
            end,
            Develops = function(_1, _2)
                return ("%s的%s能力提升了。"):format(_.name(_1), _2)
            end,
        },
    },
    NotAffectedByRotten = function(_1)
        return ("不过，%s没什么事。"):format(_.name(_1))
    end,
    PassedRotten = {
        _.quote "啊呜呜！这是什么饭菜！",
        _.quote "额！",
        _.quote "......！！",
        _.quote "嗯哦...",
        _.quote "这是什么恶作剧啊",
        _.quote "太难吃了！",
    },

    Harvesting = {
        ItemName = {
            Grown = function(weight)
                return ("%s成熟了"):format(weight)
            end,
        },
        Weight = {
            ["0"] = "超小型",
            ["1"] = "小巧",
            ["2"] = "适中",
            ["3"] = "稍微大一点",
            ["4"] = "很大",
            ["5"] = "非常巨大",
            ["6"] = "怪物大小",
            ["7"] = "大于人类",
            ["8"] = "传说中的大小",
            ["9"] = "比象还大",
        },
    },
}