Elona.Weather = {
    Feat = {
        DrawCloud = function(entity)
            return ("%s draw%s a rain cloud."):format(_.name(entity), _.s(entity))
        end,
    },

    Changes = "The weather changes.",

    Types = {
        Etherwind = {
            Starts = "Ether Wind starts to blow. You need to find a shelter!",
            Stops = "The Ether Wind dissipates.",
        },
        Rain = {
            Starts = "It starts to rain.",
            Stops = "It stops raining.",
            BecomesHeavier = "The rain becomes heavier.",
        },
        HardRain = {
            Starts = "Suddenly, rain begins to pour down from the sky.",
            BecomesLighter = "The rain becomes lighter.",
            Travel = {
                Hindered = { "It's raining heavily. You lose your way.", "You can't see a thing!" },
                Sound = { "*drip*", "*sip*", "*drizzle*", "*splash*", "*kissh*" },
            },
        },
        Snow = {
            Starts = "It starts to snow.",
            Stops = "It stops snowing.",
            Travel = {
                Hindered = {
                    "Snow delays your travel.",
                    "You are caught in a snowdrift.",
                    "It's hard to walk on a snowy road.",
                },
                Eat = "You are too hungry. You chow down snow.",
                Sound = { " *kisssh*", "*thudd*", "*siz*", "*clump*", "*skritch*" },
            },
        },
    },
}
