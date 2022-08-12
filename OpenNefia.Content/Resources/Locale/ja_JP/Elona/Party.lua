Elona.Party = {
    Recruit = {
        PartyFull = "仲間の最大数に達しているため、仲間にできなかった…",
        Success = function(entity)
            return ("%sが仲間に加わった！"):format(_.basename(entity))
        end,
    },
}
