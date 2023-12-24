Elona.Return = {
    Prompt = "Where do you want to go?",
    LevelCounter = function(level)
        return ("Lv%s"):format(level)
    end,
    NoLocations = "You don't know any location you can return to.",
    Begin = function(source)
        return ("The air around %s becomes charged."):format(_.name(source))
    end,
    Cancel = function(source)
        return ("The air around %s gradually loses power."):format(_.name(source))
    end,
    Prevented = function(source)
        return ("Strange power prevents %s from returning."):format(_.name(source))
    end,
}
