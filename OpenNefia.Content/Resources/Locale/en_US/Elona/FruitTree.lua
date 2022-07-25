Elona.FruitTree = {
    Bash = function(tree)
        return ("You throw your weight against %s."):format(_.name(tree))
    end,
    NoFruits = "It seems there are no fruits left on the tree.",
    FallsDown = function(fruit)
        return ("%s falls down from the tree."):format(_.name(fruit))
    end,
}
