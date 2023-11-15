Elona.Dialog.Common = {
    Choices = {
        Sex = "気持ちいいことしない？",
    },
    Sex = {
        Choices = {
            Confirm = "はじめる",
            GoBack = "やめる",
        },
        Prompt = function(speaker)
            return ("なかなかの体つき%sよし、買%s"):format(_.dana(speaker), _.u(speaker, 2))
        end,
        Response = "うふふ",
        Start = function(speaker)
            return ("いく%s"):format(_.yo(speaker, 2))
        end,
    },
}
