Elona.Effect = {
    Common = {
        ItIsCursed = "It's cursed!",
        CursedLaughter = function(target)
            return ("%s hear%s devils laugh."):format(_.name(target), _.s(target))
        end,
        Resists = function(target)
            return ("%s resist%s."):format(_.name(target), _.s(target))
        end,
    },

    Identify = {
        Fully = function(item)
            return ("それは%sだと完全に判明した。"):format(_.name(item))
        end,
        Partially = function(item)
            return ("それは%sだと判明したが、完全には鑑定できなかった。"):format(_.name(item))
        end,
        NeedMorePower = "新しい知識は得られなかった。より上位の鑑定で調べる必要がある。",
    },

    Uncurse = {
        Apply = {
            Normal = function(target)
                return ("%sの装備品は白い光に包まれた。"):format(_.name(target))
            end,
            Blessed = function(target)
                return ("%sは聖なる光に包み込まれた。"):format(_.name(target))
            end,
        },

        Result = {
            Equipment = "身に付けている装備の幾つかが浄化された。",
            Items = "幾つかのアイテムが浄化された。",
            Resisted = "幾つかのアイテムは抵抗した。",
        },
    },

    Curse = {
        Spell = function(source, target)
            return ("%sは%sを指差して呪いの言葉を呟いた。"):format(_.name(source), _.name(target))
        end,
        Apply = function(target, item)
            return ("%sの%sは黒く輝いた。"):format(_.name(target), _.name(item))
        end,
        NoEffect = "あなたは祈祷を捧げ呪いのつぶやきを無効にした。",
    },
}
