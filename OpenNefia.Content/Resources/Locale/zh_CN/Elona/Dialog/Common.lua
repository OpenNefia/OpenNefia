Elona.Dialog.Common = {
    PartyIsFull = function(speaker, player)
        return ("队伍已满，无法再带更多伙伴%s请调整人数后再来%s"):format(
            _.da(speaker),
            _.kure(speaker)
        )
    end,
    Choices = {
        Sex = "要做件爽事吗？",
    },
    Sex = {
        Choices = {
            Confirm = "开始",
            GoBack = "离开",
        },
        Prompt = function(speaker)
            return ("身材不错%s好，要买%s"):format(_.dana(speaker), _.u(speaker, 2))
        end,
        Response = "嘻嘻",
        Start = function(speaker)
            return ("好了%s"):format(_.yo(speaker, 2))
        end,
    },
}