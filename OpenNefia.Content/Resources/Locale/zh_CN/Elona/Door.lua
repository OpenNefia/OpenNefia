Elona.Door = {
    QueryClose = "请问要关上什么？",
    Open = {
        Succeeds = function(entity)
            return ("%s打开了门。"):format(_.name(entity))
        end,
        Fails = function(entity)
            return ("%s解锁失败。"):format(_.sore_wa(entity))
        end,
    },
    Close = {
        Succeeds = function(entity)
            return ("%s关闭了门。"):format(_.name(entity))
        end,
        Blocked = "有东西阻碍，无法关上。",
        NothingToClose = "没有可以关闭的方向。",
    },
}