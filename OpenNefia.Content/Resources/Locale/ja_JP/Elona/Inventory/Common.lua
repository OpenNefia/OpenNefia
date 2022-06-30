Elona.Inventory.Common = {
    DoesNotExist = "そのアイテムは存在しない。",
    SetAsNoDrop = "それはあなたの大事なものだ。<調べる>メニューから解除できる。",
    InventoryIsFull = "バックパックが一杯だ。",

    HowMany = function(min, max, entity)
        return ("いくつ？ (%s〜%s)"):format(min, max)
    end,

    Invalid = function(uid, protoId)
        return ("Invalid Item Id found. Item No:%s, Id:%s has been removed from your inventory."):format(uid, protoId)
    end,
}
