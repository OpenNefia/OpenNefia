Elona.Item.HouseBoard = {
    CannotUseItHere = "You can't use it here.",
    WhatDo = "What do you want to do?",

    Unlimited = "Unlimited",
    ItemCount = function(mapEntity, itemCount, furnitureCount, maxItems)
        return ("There are %s items and %s furniture in %s. (Max: %s) "):format(
            itemCount,
            furnitureCount,
            _.name(mapEntity),
            maxItems
        )
    end,

    Actions = {
        AlliesInYourHome = "Allies in your home",
        AssignABreeder = "Assign a breeder",
        AssignAShopkeeper = "Assign a shopkeeper",
        Design = "Design",
        Extend = function(_1)
            return ("Extend(%s GP)"):format(_1)
        end,
        HomeRank = "Home rank",
        MoveAStayer = "Move a stayer",
        RecruitAServant = "Recruit a servant",
    },
}
