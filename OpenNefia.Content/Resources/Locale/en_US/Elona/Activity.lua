Elona.Activity = {
    DefaultVerb = "current action",
    Cancel = {
        Normal = function(actor, activity)
            return ("%s stop%s %s."):format(_.name(actor), _.s(actor), _.name(activity))
        end,
        Item = function(actor)
            return ("%s cancel%s %s action."):format(_.name(actor), _.s(actor), _.his(actor))
        end,
        Prompt = function(activity)
            return ("Do you want to cancel %s? "):format(_.name(activity))
        end,
    },

    Resting = {
        Start = "You lie down to rest.",
        Finish = "You finished taking a rest.",
        DropOffToSleep = "After a short while, you drop off to sleep.",
    },
}
