Elona.Targeting = {
    PromptReallyAttack = function(entity)
        return ("本当に%sを攻撃する？ "):format(_.name(entity))
    end,
    NoTarget = "ターゲットが見当たらない。",
}
