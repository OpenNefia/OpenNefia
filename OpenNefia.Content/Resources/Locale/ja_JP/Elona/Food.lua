Elona.Food = {
    ItemName = {
        Rotten = "腐った",
    },
    Cook = function(oldFoodName, toolEntity, newFoodEntity)
        return ("%sで%sを料理して、%sを作った。"):format(
            _.name(toolEntity, 1),
            oldFoodName,
            _.name(newFoodEntity, 1)
        )
    end,
    EatStatus = {
        Good = function(_1)
            return ("%sは良い予感がした。"):format(_.name(_1))
        end,
        Bad = function(_1)
            return ("%sは嫌な感じがした。"):format(_.name(_1))
        end,
        CursedDrink = function(_1)
            return ("%sは気分が悪くなった。"):format(_.name(_1))
        end,
    },
    Nutrition = {
        Bloated = {
            "もう当分食べなくてもいい。",
            "こんなに食べたことはない！",
            "信じられないぐらい満腹だ！",
        },
        Satisfied = {
            "あなたは満足した。",
            "満腹だ！",
            "あなたは食欲を満たした。",
            "あなたは幸せそうに腹をさすった。",
        },
        Normal = {
            "まだ食べられるな…",
            "あなたは腹をさすった。",
            "少し食欲を満たした。",
        },
        Hungry = {
            "まだまだ食べたりない。",
            "物足りない…",
            "まだ空腹だ。",
            "少しは腹の足しになったか…",
        },
        VeryHungry = {
            "全然食べたりない！",
            "腹の足しにもならない。",
            "すぐにまた腹が鳴った。",
        },
        Starving = {
            "こんな量では意味がない！",
            "これぐらいでは、死を少し先に延ばしただけだ。",
            "無意味だ…もっと栄養をとらなければ。",
        },
    },
    Message = {
        Quality = {
            Bad = { "うぅ…腹を壊しそうだ。", "まずい！", "ひどい味だ！" },
            SoSo = { "まあまあの味だ。", "悪くない味だ。" },
            Good = { "かなりいける。", "それなりに美味しかった。" },
            Great = { "美味しい！", "これはいける！", "いい味だ！" },
            Delicious = { "最高に美味しい！", "まさに絶品だ！", "天にも昇る味だ！" },
        },
        Uncooked = { "まずいわけではないが…", "平凡な味だ。" },
        Human = {
            Delicious = "ウマイ！",
            Dislike = "これは人肉だ…うぇぇ！",
            Like = "これはあなたの大好きな人肉だ！",
            WouldHaveRatherEaten = "人肉の方が好みだが…",
        },
        RawGlum = function(_1)
            return ("%sは渋い顔をした。"):format(_.name(_1))
        end,
        Rotten = "うげっ！腐ったものを食べてしまった…うわ…",
        Ability = {
            Deteriorates = function(_1, _2)
                return ("%sの%sは衰えた。"):format(_.name(_1), _2)
            end,
            Develops = function(_1, _2)
                return ("%sの%sは発達した。"):format(_.name(_1), _2)
            end,
        },
    },
    NotAffectedByRotten = function(_1)
        return ("しかし、%sは何ともなかった。"):format(_.name(_1))
    end,
    PassedRotten = {
        _.quote "うぐぐ！なんだこの飯は！",
        _.quote "うっ！",
        _.quote "……！！",
        _.quote "あれれ…",
        _.quote "…これは何の嫌がらせですか",
        _.quote "まずい！",
    },

    Harvesting = {
        ItemName = {
            Grown = function(weight)
                return ("%s育った"):format(weight)
            end,
        },
        Weight = {
            ["0"] = "超ミニに",
            ["1"] = "小振りに",
            ["2"] = "手ごろに",
            ["3"] = "やや大きく",
            ["4"] = "どでかく",
            ["5"] = "かなり巨大に",
            ["6"] = "化け物サイズに",
            ["7"] = "人より大きく",
            ["8"] = "伝説的サイズに",
            ["9"] = "象より重く",
        },
    },
}
