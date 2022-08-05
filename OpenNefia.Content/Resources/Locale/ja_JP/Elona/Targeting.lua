Elona.Targeting = {
    PromptReallyAttack = function(entity)
        return ("本当に%sを攻撃する？ "):format(_.name(entity))
    end,
    NoTarget = "ターゲットが見当たらない。",
    NoTargetInDirection = "その方向には、操作できる対象はない。",
}
