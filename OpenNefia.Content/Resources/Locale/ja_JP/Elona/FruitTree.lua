Elona.FruitTree = {
    Bash = function(tree)
        return ("%sに体当たりした。"):format(_.name(tree))
    end,
    NoFruits = "もう実はないようだ… ",
    FallsDown = function(fruit)
        return ("%sが降ってきた。"):format(_.name(fruit))
    end,
}
