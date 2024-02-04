local function trim_job(name)
    -- TODO
    return name
end

Elona.Role = {
    Names = {
        Alien = {
            AlienKid = "异星孩子",
            Child = "孩子",
            ChildOf = function(_1)
                return ("的孩子：%s"):format(_1)
            end,
        },
        Fanatic = { "奥帕托斯狂热者", "玛妮狂热者", "艾卡托尔狂热者" },
        HorseMaster = function(_1)
            return ("%s 马术大师"):format(trim_job(_1))
        end,
        OfDerphy = function(_1)
            return ("%s 来自德斐"):format(_1)
        end,
        OfLumiest = function(_1)
            return ("%s 来自卢米斯特"):format(_1)
        end,
        OfNoyel = function(_1)
            return ("%s 来自诺伊尔"):format(_1)
        end,
        OfPalmia = function(_1)
            return ("%s 来自帕尔米亚城"):format(_1)
        end,
        OfPortKapul = function(_1)
            return ("%s 来自卡普尔港"):format(_1)
        end,
        OfVernis = function(_1)
            return ("%s 来自维尼斯"):format(_1)
        end,
        OfYowyn = function(_1)
            return ("%s 来自约温"):format(_1)
        end,
        Shade = "暗影",
        SlaveMaster = "奴隶主",
        SpellWriter = function(_1)
            return ("%s 写法师"):format(trim_job(_1))
        end,
        Trainer = function(_1)
            return ("%s 训练师"):format(trim_job(_1))
        end,
    },
}