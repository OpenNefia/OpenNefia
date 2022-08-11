Elona.MusicDisc = {
    YouPlay = function(user, item)
        return ("%s play%s %s."):format(_.name(user), _.s(user), _.name(item))
    end,
}
