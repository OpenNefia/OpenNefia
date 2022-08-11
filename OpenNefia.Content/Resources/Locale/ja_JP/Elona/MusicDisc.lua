Elona.MusicDisc = {
    YouPlay = function(user, item)
        return ("%s%sを再生した。"):format(_.sore_wa(user), _.name(item))
    end,
}
