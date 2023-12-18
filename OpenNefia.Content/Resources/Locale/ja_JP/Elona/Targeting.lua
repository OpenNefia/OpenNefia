Elona.Targeting = {
    PromptReallyAttack = function(entity)
        return ("本当に%sを攻撃する？ "):format(_.name(entity))
    end,
    NoTarget = "ターゲットが見当たらない。",
    NoTargetInDirection = "その方向には、操作できる対象はない。",

    Action = {
        FindNothing = "視界内にターゲットは存在しない。",
        YouTarget = function(onlooker, target)
            return ("%s%sをターゲットにした。"):format(_.sore_wa(onlooker), _.name(target))
        end,
        YouTargetGround = function(onlooker)
            return ("%s地面をターゲットにした。"):format(_.sore_wa(onlooker))
        end,
    },

    Prompt = {
        WhichDirection = "どの方向？",
        InWhichDirection = "どの方向に？",
        CannotSeeLocation = "その場所は見えない。",
        OutOfRange = "射程距離外だ。",
    },
}
