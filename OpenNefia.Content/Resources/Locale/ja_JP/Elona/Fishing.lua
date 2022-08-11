Elona.Fishing = {
    ItemName = {
        Bait = function(name, baitName)
            return ("%s%s"):format(name, baitName)
        end,
        FishingPole = function(name, bait, charges)
            return ("%s(%s残り%s匹)"):format(name, bait, charges)
        end,
    },
}
