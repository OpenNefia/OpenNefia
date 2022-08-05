Elona.Targeting = {
    PromptReallyAttack = function(entity)
        return ("Really attack %s?"):format(_.name(entity))
    end,
    NoTarget = "You find no target.",
    NoTargetInDirection = "There's no valid target in that direction.",
}
