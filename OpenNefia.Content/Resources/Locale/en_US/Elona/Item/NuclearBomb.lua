Elona.Item.NuclearBomb = {
    CannotPlaceHere = "You can't place it here.",
    PromptNotQuestGoal = "This location is not your quest goal. Really place it here?",
    SetUp = function(entity, item)
        return ("%s set%s up the nuke...now run!!"):format(_.name(entity), _.s(entity))
    end,
}
