Elona.Movement = {
    Displace = {
        Text = function(source, target)
            return ("%s displace%s %s."):format(_.name(source), _.s(source), _.name(target))
        end,
        Dialog = { "Oops, sorry.", "Watch it." },
        InterruptActivity = function(source, target)
            return ("%s stares in %s face."):format(_.name(target), _.his_named(source))
        end,
    },
}
