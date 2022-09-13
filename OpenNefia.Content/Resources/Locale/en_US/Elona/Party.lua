Elona.Party = {
    Recruit = {
        PartyFull = "Your party is already full. You can't invite someone anymore.",
        Success = function(entity)
            return ("%s join%s your party!"):format(_.name(entity), _.s(entity))
        end,
    },
}
