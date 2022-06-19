Elona.Religion = {
    Menu = {
        Ability = function(_1)
            return ("特殊能力: %s"):format(_1)
        end,
        Bonus = function(_1)
            return ("ボーナス: %s"):format(_1)
        end,
        Offering = function(_1)
            return ("　捧げ物: %s"):format(_1)
        end,
        Window = {
            Abandon = "信仰を捨てる",
            Believe = function(godName)
                return ("%sを信仰する"):format(godName)
            end,
            Cancel = "やめる",
            Convert = function(godName)
                return ("%sに改宗する"):format(godName)
            end,
            Title = function(godName)
                return ("《 %s 》"):format(godName)
            end,
        },
    },
    Enraged = function(godName)
        return ("%sは激怒した。"):format(godName)
    end,
    Indifferent = "あなたの信仰は既に限界まで高まっている。",
    Pray = {
        DoNotBelieve = function()
            return ("%sは神を信仰していないが、試しに祈ってみた。"):format(_.you())
        end,
        Indifferent = function(godName)
            return ("%sはあなたに無関心だ。"):format(godName)
        end,
        Prompt = "あなたの神に祈りを乞う？",
        Servant = {
            NoMore = "神の使徒は2匹までしか仲間にできない。",
            PartyIsFull = "仲間が一杯で、神からの贈り物を受け取ることができなかった。",
            PromptDecline = "この贈り物を諦める？",
        },
        YouPrayTo = function(godName)
            return ("%sに祈った。"):format(godName)
        end,
    },
    Switch = {
        Follower = function(godName)
            return ("あなたは今や%sの信者だ！"):format(godName)
        end,
        Unbeliever = "あなたは今や無信仰者だ。",
    },
    Offer = {
        Claim = function(godName)
            return ("異世界で、%sが空白の祭壇の権利を主張した。"):format(godName)
        end,
        DoNotBelieve = "あなたは神を信仰していないが、試しに捧げてみた。",
        Execute = function(item, godName)
            return ("あなたは%sを%sに捧げ、その名を唱えた。"):format(item, godName)
        end,
        Result = {
            Best = function(item)
                return ("%sはまばゆく輝いて消えた。"):format(_1)
            end,
            Good = function(item)
                return ("%sは輝いて消え、三つ葉のクローバーがふってきた。"):format(item)
            end,
            Okay = function(item)
                return ("%sは一瞬輝いて消えた。"):format(item)
            end,
            Poor = function(item)
                return ("%sは消えた。"):format(item)
            end,
        },
        TakeOver = {
            Attempt = function(godName, otherGodName)
                return ("異様な霧が現れ、%sと%sの幻影がせめぎあった。"):format(
                    godName,
                    otherGodName
                )
            end,
            Fail = function(godName)
                return ("%sは祭壇を守りきった。"):format(godName)
            end,
            Shadow = "あなたの神の幻影は、次第に色濃くなった。",
            Succeed = function(godName, altar)
                return ("%sは%sを支配した。"):format(godName, altar)
            end,
        },
    },
}
