Elona.Targeting = {
    PromptReallyAttack = function(entity)
        return ("Really attack %s?"):format(_.name(entity))
    end,
    NoTarget = "You find no target.",
    NoTargetInDirection = "There's no valid target in that direction.",

    Action = {
        FindNothing = function(onlooker)
            return ("%s look%s around and find%s nothing."):format(_.name(onlooker))
        end,
        YouTarget = function(onlooker, target)
            return ("%s target%s %s."):format(_.name(onlooker), _.s(onlooker), _.name(target))
        end,
        YouTargetGround = "You target the ground.",
    },

    Prompt = {
        WhichDirection = "Which direction?",
        InWhichDirection = "Which direction? ",
        CannotSeeLocation = "You can't see the location.",
        OutOfRange = "It's out of range.",
    },
}
