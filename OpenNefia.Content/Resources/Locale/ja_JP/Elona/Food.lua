Elona.Food = {
    Cook = function(_1, _2, _3)
        return ("%sで%sを料理して、%sを作った。"):format(_2, _1, _3)
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
        SustainsGrowth = function(_1, _2)
            return ("%sの%sは成長期に突入した。"):format(_.name(_1), _2)
        end,
    },
    NotAffectedByRotten = function(_1)
        return ("しかし、%sは何ともなかった。"):format(_.name(_1))
    end,
    PassedRotten = {
        "「うぐぐ！なんだこの飯は！」",
        "「うっ！」",
        "「……！！」",
        "「あれれ…」",
        "「…これは何の嫌がらせですか」",
        "「まずい！」",
    },
}
