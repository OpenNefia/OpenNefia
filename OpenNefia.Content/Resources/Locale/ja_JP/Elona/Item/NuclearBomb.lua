Elona.Item.NuclearBomb = {
    CannotPlaceHere = "ここでは使えない。",
    PromptNotQuestGoal = "ここはクエストの目標位置ではない。本当にここに設置する？",
    SetUp = function(source, item)
        return ("%s原子爆弾を設置した。逃げろォー！"):format(_.sore_wa(source))
    end,
}
