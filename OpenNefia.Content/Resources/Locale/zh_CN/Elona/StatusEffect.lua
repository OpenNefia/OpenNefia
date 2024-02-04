Elona.StatusEffect = {
    Drunk = {
        Stagger = " *蹒跚* ",
        Dialog = {
            _.quote "要不要来一杯？",
            _.quote "没喝哦",
            _.quote "在看什么呢",
            _.quote "一起玩吧",
        },
        Annoyed = {
            Dialog = "「实在受够了醉鬼！」",
            Text = function(entity)
                return ("%s有点生气。"):format(_.name(entity))
            end,
        },
        GetsTheWorse = function(entity, target)
            return ("%s喝醉了，对%s进行了纠缠。"):format(_.name(entity), _.name(target))
        end,
    },
}