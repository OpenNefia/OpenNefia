Elona.Dialog.Common = {
    Thanks = function(speaker)
        return ("%s"):format(_.thanks(speaker, 2))
    end,
    YouKidding = function(speaker)
        return ("冷やかし%s"):format(_.ka(speaker, 1))
    end,

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
