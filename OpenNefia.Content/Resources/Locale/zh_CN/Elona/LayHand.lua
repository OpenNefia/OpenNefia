Elona.LayHand = {
    Dialog = function(healer)
        return ("%s大喊道：“给这个人加上朱阿的庇佑吧。雷亨德！”"):format(_.name(healer))
    end,
    IsHealed = function(target)
        return ("%s已经恢复了。"):format(_.name(target))
    end,
}