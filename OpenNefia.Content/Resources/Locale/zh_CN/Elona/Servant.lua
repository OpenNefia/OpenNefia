Elona.Servant = {
    Count = function(curServants, maxServants)
        return ("当前有%s名住客（最大%s人）"):format(curServants, maxServants)
    end,

    Hire = {
        TooManyGuests = "家里已经满员了。",
        Prompt = "雇佣谁？",
        NotEnoughMoney = "金钱不足…",
        YouHire = function(entity)
            return ("你雇佣了%s。"):format(_._.basename(entity))
        end,

        Topic = {
            InitCost = "雇佣费用（工资）",
            Wage = "工资",
        },
    },

    Move = {
        Prompt = {
            Who = "要移动谁？",
            Where = function(entity)
                return ("把%s移动到哪里？"):format(_.basename(entity))
            end,
        },
        Invalid = "无法将其移动到该位置。",
        DontTouchMe = function(entity)
            return ("%s「别碰我！」"):format(_.basename(entity))
        end,
        IsMoved = function(entity)
            return ("已将%s移动。"):format(_.basename(entity))
        end,
    },
}