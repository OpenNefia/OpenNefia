Elona.Dialog.Prostitute = {
    Choices = {
        Buy = "I'll buy you.",
    },
    Buy = {
        Text = function(speaker, cost)
            return ("Okay sweetie, I need %s gold pieces in front."):format(cost)
        end,
    },
}
