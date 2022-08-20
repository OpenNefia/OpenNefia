Elona.StatusEffect = {
    Drunk = {
        Stagger = " *ふらり* ",
        Dialog = {
            _.quote "一杯どうだい？",
            _.quote "飲んでないよ",
            _.quote "何見てるのさ",
            _.quote "遊ぼうぜ",
        },
        Annoyed = {
            Dialog = "「酔っ払いにはうんざり！」",
            Text = function(entity)
                return ("%sはカチンときた。"):format(_.name(entity))
            end,
        },
        GetsTheWorse = function(entity, target)
            return ("%sは酔っ払って%sにからんだ。"):format(_.name(entity), _.name(target))
        end,
    },
}
