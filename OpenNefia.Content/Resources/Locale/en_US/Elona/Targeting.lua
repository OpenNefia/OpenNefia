Elona.Targeting = {
    PromptReallyAttack = function(entity)
        return ("Really attack %s?"):format(_.name(entity))
    end,
    NoTarget = "You find no target.",
}
