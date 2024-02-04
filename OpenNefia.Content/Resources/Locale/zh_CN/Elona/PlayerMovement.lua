Elona.PlayerMovement = {
    SenseSomething = "感觉到地面上有些东西。",

    PromptLeaveMap = function(map)
        return ("是否要离开%s？"):format(_.name(map))
    end,
}