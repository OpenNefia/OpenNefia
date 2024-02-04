Elona.Religion = {
    Menu = {
        Ability = function(_1)
            return ("特殊能力: %s"):format(_1)
        end,
        Bonus = function(_1)
            return ("奖励: %s"):format(_1)
        end,
        Offering = function(_1)
            return (" 供奉: %s"):format(_1)
        end,
        Window = {
            Abandon = "放弃信仰",
            Believe = function(godName)
                return ("信仰 %s"):format(godName)
            end,
            Cancel = "取消",
            Convert = function(godName)
                return ("改宗为 %s"):format(godName)
            end,
            Title = function(godName)
                return ("《 %s 》"):format(godName)
            end,
        },
    },
    Enraged = function(godName)
        return ("%s变得愤怒。"):format(godName)
    end,
    Indifferent = "你的信仰已经达到上限。",
    Pray = {
        DoNotBelieve = function()
            return ("虽然%s不信任神，但是试着祈祷。"):format(_.you())
        end,
        Indifferent = function(godName)
            return ("%s对你无动于衷。"):format(godName)
        end,
        Prompt = "向你的神祈祷？",
        Servant = {
            NoMore = "你最多只能拥有两个神的仆役。",
            PartyIsFull = "队伍已满，无法接受神的礼物。",
            PromptDecline = "放弃这个礼物？",
        },
        YouPrayTo = function(godName)
            return ("你向%s祈祷。"):format(godName)
        end,
    },
    Switch = {
        Follower = function(godName)
            return ("你现在是%s的追随者！"):format(godName)
        end,
        Unbeliever = "你现在是无神论者。",
    },
    Offer = {
        Claim = function(godName)
            return ("在异世界中，%s宣称对空白祭坛拥有使用权。"):format(godName)
        end,
        DoNotBelieve = "虽然你不信仰神，但是试着献祭。",
        Execute = function(item, godName)
            return ("你献祭了%s给%s，并念出了神的名字。"):format(item, godName)
        end,
        Result = {
            Best = function(item)
                return ("%s闪耀着耀眼的光芒消失了。"):format(item)
            end,
            Good = function(item)
                return ("%s闪耀着光芒消失了，落下了一片三叶草。"):format(item)
            end,
            Okay = function(item)
                return ("%s闪耀了一下就消失了。"):format(item)
            end,
            Poor = function(item)
                return ("%s消失了。"):format(item)
            end,
        },
        TakeOver = {
            Attempt = function(godName, otherGodName)
                return ("奇怪的雾气出现了，%s和%s的幻影战斗在一起。"):format(
                    godName,
                    otherGodName
                )
            end,
            Fail = function(godName)
                return ("%s保卫了祭坛。"):format(godName)
            end,
            Shadow = "你的神的幻影逐渐变得更浓。",
            Succeed = function(godName, altar)
                return ("%s控制了%s。"):format(godName, altar)
            end,
        },
    },
}