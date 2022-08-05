Elona.SandBag = {
    Interact = {
        Release = function(source, target)
            return ("%s release%s %s."):format(_.name(source), _.s(source), _.name(target))
        end,
    },
    Dialog = {
        TurnStart = {
            _.quote "Release me now.",
            _.quote "I won't forget this.",
            _.quote "Hit me!",
        },
        Damage = {
            _.quote "Kill me already!",
            _.quote "No..not yet...!",
            _.quote "I can't take it anymore...",
            _.quote "Argh!",
            _.quote "Uhhh",
            _.quote "Ugggg",
        },
    },
}
