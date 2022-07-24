Elona.EngineMessage = {
    PlayTime = {
        Report = function(hoursPlayed)
            return ("You have been playing Elona for %s hour%s."):format(hoursPlayed, _.s(hoursPlayed))
        end,
        WarnMessage = {
            ["1"] = "Larnneire cheers, \"Way to go!\"",
            ["2"] = "Lomias grins, \"Go for it.\"",
            ["3"] = "Kumiromi worries, \"Are you...okay..?\"",
            ["4"] = "Lulwy sneers, \"You're tougher than I thought, little kitty.\"",
            ["5"] = "Larnneire cries, \"No...before it is too late...\"",
            ["6"] = "Lomias grins, \"It hasn't even started yet... has it?\"",
            ["7"] = "Lulwy warns you, \"Have a rest, kitty. If you are broken, you're no use to me.\"",
            ["8"] = "Lulwy laughs, \"I guess there's no use warning you. Alright. Do as you please, kitty.\"",
            ["12"] = "Opatos laughs, \"Muwahahahahahaha!\"",
            ["24"] = "Ehekatl hugs you, \"Don't die! Promise you don't die!\"",
        },
    },
}
