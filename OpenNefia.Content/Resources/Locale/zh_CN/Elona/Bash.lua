Elona.Bash = {
    Prompt = "选择向哪个方向冲撞？",
    Air = function(basher)
        return ("%s冲撞了空气。"):format(_.name(basher))
    end,
    Execute = function(basher, target)
        return ("%s冲撞了%s。"):format(_.name(basher), _.name(target))
    end,
    DisturbsSleep = function(basher, target)
        return ("%s打扰了%s的睡眠。"):format(_.name(basher), _.name(target))
    end,
    Choking = {
        Dialog = _.quote "谢谢救命啊！",
        Execute = function(basher, target)
            return ("%s全力冲撞了%s。"):format(_.name(basher), _.name(target))
        end,
        Spits = function(target)
            return ("%s吐出了口水。"):format(_.name(target))
        end,
    },
}