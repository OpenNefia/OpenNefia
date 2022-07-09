Elona.Food.Effect = {
    Poisoned = {
        Dialog = { "「ギャァァ…！」", "「ブッ！」" },
        Text = function(_1)
            return ("これは毒されている！%sはもがき苦しみのたうちまわった！"):format(
                _.name(_1)
            )
        end,
    },
    Spiked = {
        Other = {
            function(_1)
                return ("%s「なんだか…変な気分なの…」"):format(_.name(_1))
            end,
            function(_1)
                return ("%s「あれ…なにこの感じは…」"):format(_.name(_1))
            end,
        },
        Self = "あなたは興奮した！",
    },

    Corpse = {
        Alien = function(_1)
            return ("何かが%sの体内に入り込んだ。"):format(_.name(_1))
        end,
        At = "＠を食べるなんて…",
        Beetle = "力が湧いてくるようだ。",
        Calm = "この肉は心を落ち着かせる効果があるようだ。",
        Cat = "猫を食べるなんて！！",
        ChaosCloud = function(_1)
            return ("%sの胃は混沌で満たされた。"):format(_.name(_1))
        end,
        CupidOfLove = function(_1)
            return ("%sは恋をしている気分になった！"):format(_.name(_1))
        end,
        DeformedEye = "気が変になりそうな味だ。",
        Ether = function(_1)
            return ("%sの体内はエーテルで満たされた。"):format(_.name(_1))
        end,
        Ghost = "精神が少しずぶとくなった。",
        Giant = "体力がつきそうだ。",
        Grudge = "胃の調子がおかしい…",
        Guard = "ガード達はあなたを憎悪した。",
        HolyOne = function(_1)
            return ("%sは神聖なものを汚した気がした。"):format(_.name(_1))
        end,
        Horse = "馬肉だ！これは精がつきそうだ。",
        Imp = "魔力が鍛えられる。",
        Insanity = function(_1)
            return ("%sの胃は狂気で満たされた。"):format(_.name(_1))
        end,
        Iron = function(_1)
            return ("まるで鉄のように硬い！%sの胃は悲鳴をあげた。"):format(_.name(_1))
        end,
        Lightning = function(_1)
            return ("%sの神経に電流が走った。"):format(_.name(_1))
        end,
        Mandrake = "微かな魔力の刺激を感じた。",
        Poisonous = "これは有毒だ！",
        Putit = "肌がつるつるになりそうだ。",
        Quickling = function(_1)
            return ("ワアーォ、%sは速くなった気がする！"):format(_.name(_1))
        end,
        RottenOne = "腐ってるなんて分かりきっていたのに…うげぇ",
        Strength = "力がつきそうだ。",
        Troll = "血が沸き立つようだ。",
        Vesda = function(_1)
            return ("%sの体は一瞬燃え上がった。"):format(_.name(_1))
        end,
        LittleSister = function(_1)
            return ("%sは進化した。"):format(_.name(_1))
        end,
    },
    FortuneCookie = function(_1)
        return ("%sはクッキーの中のおみくじを読んだ。"):format(_.name(_1))
    end,
    Mochi = {
        Chokes = function(_1)
            return ("%sはもちを喉につまらせた！"):format(_.name(_1))
        end,
        Dialog = _.quote "むがっ",
    },
    Herb = {
        Alraunia = "ホルモンが活発化した。",
        Curaria = "このハーブは活力の源だ。",
        Mareilon = "魔力の向上を感じる。",
        Morgia = "新たな力が湧きあがってくる。",
        Spenseweed = "感覚が研ぎ澄まされるようだ。",
    },
    KagamiMochi = "これは縁起がいい！",
    FairySeed = function(_1, _2)
        return ("「げふぅ」%sは%sを吐き出した。"):format(_.name(_1), _2)
    end,
    SistersLoveFueledLunch = function(_1)
        return ("%sの心はすこし癒された。"):format(_.name(_1))
    end,
}
