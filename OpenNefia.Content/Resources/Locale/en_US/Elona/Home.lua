Elona.Home = {
    Map = {
        Name = "Your Home",
        Description = "It's your sweet home.",
    },
    ItemName = {
        Deed = function(home_name)
            return ("of %s"):format(home_name)
        end,
    },
    WelcomeHome = {
        _.quote "Welcome home!",
        _.quote "Hey, dear.",
        _.quote "You're back!",
        _.quote "I was waiting for you.",
        _.quote "Nice to see you again.",
    },
}
