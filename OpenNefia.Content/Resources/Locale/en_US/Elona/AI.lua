Elona.AI = {
    Ally = {
        SellsItems = function(entity, itemCount, goldEarned)
            return ("%s sells %s item%s and earns %s gold piece%s."):format(
                _.name(entity),
                itemCount,
                _.s(itemCount),
                goldEarned,
                _.s(goldEarned)
            )
        end,
        VisitsTrainer = function(_1)
            return ("%s visits a trainer and develops %s potential!"):format(_.basename(_1), _.his(_1))
        end,
    },
    CrushesWall = function(_1)
        return ("%s crush%s the wall!"):format(_.name(_1), _.s(_1, true))
    end,
    FireGiant = {
        _.quote "Filthy monster!",
        _.quote "Go to hell!",
        _.quote "I'll get rid of you.",
        _.quote "Eat this!",
    },
    MakesSnowman = function(_1, _2)
        return ("%s make%s %s!"):format(_.name(_1), _.s(_1), _2)
    end,
    Snail = { _.quote "Snail!", _.quote "Kill!" },
    Snowball = {
        "*grin*",
        _.quote "Fire in the hole!",
        _.quote "Tee-hee-hee!",
        _.quote "Eat this!",
        _.quote "Watch out!",
        _.quote "Scut!",
    },
    Swap = {
        Displaces = function(_1, _2)
            return ("%s displace%s %s."):format(_.name(_1), _.s(_1), _.name(_2))
        end,
        Glares = function(_1, _2)
            return ("%s glare%s at %s."):format(_.name(_2), _.s(_2), _.name(_1))
        end,
    },
}
