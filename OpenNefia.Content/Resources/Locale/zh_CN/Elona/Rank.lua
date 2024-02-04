Elona.Rank = {
    Changed = function(rankName, oldPlace, newPlace, title)
        return ("排名变动（%s %s位 → %s位）《%s》"):format(rankName, oldPlace, newPlace, title)
    end,
    CloserToNextRank = "正在稳步接近下一个等级。",
}
