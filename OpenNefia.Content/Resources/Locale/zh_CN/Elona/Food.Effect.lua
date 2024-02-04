Elona.Food.Effect = {
    Poisoned = {
        Dialog = { "「呕呕呕！」", "「啊！」" },
        Text = function(_1)
            return ("你中毒了！%s痛苦地扭动着！"):format(
                _.name(_1)
            )
        end,
    },
    Spiked = {
        Other = {
            function(_1)
                return ("%s「感觉好奇怪…」"):format(_.name(_1))
            end,
            function(_1)
                return ("%s「这种感觉是什么…」"):format(_.name(_1))
            end,
        },
        Self = "你感到兴奋！",
    },

    Corpse = {
        Alien = function(_1)
            return ("有什么东西进入了%s的体内。"):format(_.name(_1))
        end,
        At = "竟然吃掉了＠...",
        Beetle = "你感觉到力量湧动。",
        Calm = "这块肉似乎具有宁心的效果。",
        Cat = "竟然吃掉猫！！",
        ChaosCloud = function(_1)
            return ("%s的胃被混沌所填满。"):format(_.name(_1))
        end,
        CupidOfLove = function(_1)
            return ("%s的心情被爱情所陶醉！"):format(_.name(_1))
        end,
        DeformedEye = "这个味道让人感到不舒服。",
        Ether = function(_1)
            return ("%s的体内充满了以太。"):format(_.name(_1))
        end,
        Ghost = "精神变得有点迟钝。",
        Giant = "体力似乎快消耗完了。",
        Grudge = "胃里感觉不舒服...",
        Guard = "卫兵们憎恨着你。",
        HolyOne = function(_1)
            return ("%s感觉自己亵渎了神圣之物。"):format(_.name(_1))
        end,
        Horse = "马肉！感觉精神焕发。",
        Imp = "魔力得到了锻炼。",
        Insanity = function(_1)
            return ("%s的胃被疯狂所填满。"):format(_.name(_1))
        end,
        Iron = function(_1)
            return ("这块肉就像铁一样硬！%s的胃发出了惊叫声。"):format(_.name(_1))
        end,
        Lightning = function(_1)
            return ("%s的神经感受到了电流。"):format(_.name(_1))
        end,
        Mandrake = "感受到微弱的魔力刺激。",
        Poisonous = "这个有毒！",
        Putit = "皮肤感觉变得光滑。",
        Quickling = function(_1)
            return ("哇啊，%s感觉变得更快了！"):format(_.name(_1))
        end,
        RottenOne = "明明就知道已经腐烂了...呕...",
        Strength = "力量似乎快消耗完了。",
        Troll = "感到热血沸腾。",
        Vesda = function(_1)
            return ("%s的身体瞬间燃烧起来。"):format(_.name(_1))
        end,
        LittleSister = function(_1)
            return ("%s进化了。"):format(_.name(_1))
        end,
    },
    FortuneCookie = function(_1)
        return ("%s阅读了饼干里的预言。"):format(_.name(_1))
    end,
    Mochi = {
        Chokes = function(_1)
            return ("%s的喉咙被麻麦噎住了！"):format(_.name(_1))
        end,
        Dialog = _.quote "咯咯",
    },
    Herb = {
        Alraunia = "荷尔蒙被振奋起来。",
        Curaria = "这种草药是活力的源泉。",
        Mareilon = "感觉到魔力的提升。",
        Morgia = "新的力量涌现。",
        Spenseweed = "感觉到感官变得更加敏锐。",
    },
    KagamiMochi = "这是好兆头！",
    FairySeed = function(_1, _2)
        return ("「咳咳」%s吐出了%s。"):format(_.name(_1), _2)
    end,
    SistersLoveFueledLunch = function(_1)
        return ("%s的心情稍微得到了安抚。"):format(_.name(_1))
    end,
}