Elona.Targeting = {
    PromptReallyAttack = function(entity)
        return ("确定要攻击%s吗？"):format(_.name(entity))
    end,
    NoTarget = "找不到目标。",
    NoTargetInDirection = "该方向没有可操作的目标。",

    Action = {
        FindNothing = "视野范围内没有找到目标。",
        YouTarget = function(onlooker, target)
            return ("%s将%s设为目标。"):format(_.sore_wa(onlooker), _.name(target))
        end,
        YouTargetGround = function(onlooker)
            return ("%s将地面设为目标。"):format(_.sore_wa(onlooker))
        end,
    },

    Prompt = {
        WhichDirection = "选择哪个方向？",
        InWhichDirection = "选择哪个方向？",
        CannotSeeLocation = "无法看到该位置。",
        OutOfRange = "超出射程范围。",
    },
}