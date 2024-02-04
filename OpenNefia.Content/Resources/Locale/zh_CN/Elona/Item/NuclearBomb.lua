Elona.Item.NuclearBomb = {
    CannotPlaceHere = "无法在这里放置。",
    PromptNotQuestGoal = "这不是任务目标位置。确定要在这里放置吗？",
    SetUp = function(source, item)
        return ("将%s原子弹设置好了。快跑啊！"):format(_.sore_wa(source))
    end,
}