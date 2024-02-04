Elona.Dialog.Guard = {
    Choices = {
        WhereIs = function(entity)
            return ("询问%s的位置"):format(_.name(entity, true))
        end,
        LostProperty = function(item)
            return ("送还失物%s"):format(_.basename(item))
        end,
    },

    LostProperty = {
        TurnIn = {
            Dialog = function(speaker)
                return ("特地送还失物的%s%s是市民的模范%s%s"):format(
                    _.noka(speaker),
                    _.kimi(speaker, 3),
                    _.da(speaker),
                    _.thanks(speaker)
                )
            end,
            Choice = "当然的事",
        },
        Empty = {
            Dialog = function(speaker)
                return ("嗯…里面是空的%s"):format(_.dana(speaker, 2))
            end,
            Choice = "糟糕…",
        },
        FoundOften = {
            Dialog = {
                function(speaker)
                    return ("嗯，%s%s经常找到钱包%s"):format(
                        _.kimi(speaker, 3),
                        _.ka(speaker),
                        _.dana(speaker)
                    )
                end,
                "（…可疑）",
            },
            Choice = "吓了一跳",
        },
    },

    WhereIs = {
        Dead = function(speaker)
            return ("那家伙现在已经死了%s"):format(_.yo(speaker, 2))
        end,
        VeryClose = function(speaker, target, direction)
            return ("%s就在附近%s%s看向那个方向。"):format(
                _.name(target, true),
                _.yo(speaker),
                direction
            )
        end,
        Close = function(speaker, target, direction)
            return ("刚刚在%s的方向看到%s"):format(direction, _.yo(speaker))
        end,
        Moderate = function(speaker, target, direction)
            return ("%s的话，就朝%s的方向寻找吧。"):format(_.name(target, true), direction)
        end,
        Far = function(speaker, target, direction)
            return ("如果想见%s，需要朝%s走相当远%s"):format(
                _.name(target, true),
                direction,
                _.ru(speaker)
            )
        end,
        VeryFar = function(speaker, target, direction)
            return ("%s%s，从这里%s的地方肯定相当遥远%s"):format(
                _.name(target, true),
                _.ka(speaker, 3),
                direction,
                _.da(speaker)
            )
        end,
    },
}