Elona.StatusEffect = {
    Drunk = {
        Stagger = "*stagger*",
        Dialog = {
            _.quote "Have a drink baby.",
            _.quote "What are you looking at?",
            _.quote "I ain't drunk.",
            _.quote "Let's have fun.",
        },
        Annoyed = {
            Dialog = "Your time is over, drunk!",
            Text = function(entity)
                return ("%s is pretty annoyed with the drunkard."):format(_.name(entity))
            end,
        },
        GetsTheWorse = function(entity, target)
            return ("%s gets the worse for drink and catches %s."):format(_.name(entity), _.name(target))
        end,
    },
}
