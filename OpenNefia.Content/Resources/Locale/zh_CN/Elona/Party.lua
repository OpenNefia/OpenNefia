Elona.Party = {
    Recruit = {
        PartyFull = "由于队伍已满，无法招募队友...",
        Success = function(entity)
            return ("%s加入了队伍！"):format(_.name(entity))
        end,
    },
}