Elona.FruitTree = {
    Bash = function(tree)
        return ("对%s进行了撞击。"):format(_.name(tree))
    end,
    NoFruits = "好像没有果实了...",
    FallsDown = function(fruit)
        return ("掉下了%s。"):format(_.name(fruit))
    end,
}