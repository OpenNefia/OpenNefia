Elona.MusicDisc = {
    YouPlay = function(user, item)
        return ("%s播放了%s。"):format(_.sore_wa(user), _.name(item))
    end,
}