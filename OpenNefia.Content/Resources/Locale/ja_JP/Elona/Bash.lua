Elona.Bash = {
    Prompt = "どの方向に体当たりする？ ",
    Air = function(basher)
        return ("%sは空気に体当たりした。"):format(_.name(basher))
    end,
    Execute = function(basher, target)
        return ("%sは%sに体当たりした。"):format(_.name(basher), _.name(target))
    end,
    DisturbsSleep = function(basher, target)
        return ("%sは睡眠を妨害された。"):format(_.name(target))
    end,
    Choking = {
        Dialog = _.quote "助かったよ！",
        Execute = function(basher, target)
            return ("%sは%sに全力で体当たりした。"):format(_.name(basher), _.name(target))
        end,
        Spits = function(target)
            return ("%sはもちを吐き出した。"):format(_.name(target))
        end,
    },
}
