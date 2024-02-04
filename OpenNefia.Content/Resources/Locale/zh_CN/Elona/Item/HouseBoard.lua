Elona.Item.HouseBoard = {
    CannotUseItHere = "无法在此处使用。",
    WhatDo = "你要做什么？",

    Unlimited = "无限",
    ItemCount = function(mapEntity, itemCount, furnitureCount, maxItems)
        return ("%s有%s个物品和%s个家具（物品最多%s个）"):format(
            _.name(mapEntity),
            itemCount,
            furnitureCount,
            maxItems
        )
    end,

    Actions = {
        AlliesInYourHome = "盟友在你的家中",
        AssignABreeder = "任命一个养殖员",
        AssignAShopkeeper = "请盟友担任店主",
        Design = "设计家居",
        Extend = function(goldCost)
            return ("扩建店铺（花费%s金币）"):format(goldCost)
        end,
        HomeRank = "家的信息",
        MoveAStayer = "移动居住者",
        RecruitAServant = "招募仆人",
    },
}