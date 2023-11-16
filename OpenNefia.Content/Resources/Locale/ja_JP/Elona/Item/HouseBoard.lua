Elona.Item.HouseBoard = {
    CannotUseItHere = "それはここでは使えない。",
    WhatDo = "何をする？",

    Unlimited = "(無限)",
    ItemCount = function(mapEntity, itemCount, furnitureCount, maxItems)
        return ("%sには%s個のアイテムと%s個の家具がある(アイテム最大%s個) "):format(
            _.name(mapEntity),
            itemCount,
            furnitureCount,
            maxItems
        )
    end,

    Actions = {
        AlliesInYourHome = "仲間の滞在",
        AssignABreeder = "ブリーダーを任命する",
        AssignAShopkeeper = "仲間に店主を頼む",
        Design = "家の模様替え",
        Extend = function(goldCost)
            return ("店を拡張(%s GP)"):format(goldCost)
        end,
        HomeRank = "家の情報",
        MoveAStayer = "滞在者の移動",
        RecruitAServant = "使用人を募集する",
    },
}
