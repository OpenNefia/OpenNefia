Elona.Inventory.Common = {
    DoesNotExist = "该物品不存在。",
    SetAsNoDrop = "该物品非常重要，无法丢弃。你可以在“检查”菜单中解除。",
    InventoryIsFull = "背包已满。",

    HowMany = function(min, max, entity)
        return ("要多少个？（%s〜%s）"):format(min, max)
    end,

    Invalid = function(uid, protoId)
        return ("找到无效的物品ID。物品编号：%s，ID：%s 已从你的背包中移除。"):format(uid, protoId)
    end,

    SomethingFalls = {
        FromBackpack = function(item, owner)
            return ("%s从%s掉落到地面上。"):format(_.name(item), _.name(owner))
        end,
        AndDisappears = "有东西掉在地面上然后消失了...",
    },

    NameModifiers = {
        Ground = "(地面)",
    },
}