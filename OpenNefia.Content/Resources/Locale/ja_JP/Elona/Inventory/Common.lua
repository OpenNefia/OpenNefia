Elona.Inventory.Common =
{
    DoesNotExist = "そのアイテムは存在しない。",
    SetAsNoDrop = "それはあなたの大事なものだ。<調べる>メニューから解除できる。",
    InventoryIsFull = "バックパックが一杯だ。",

    Invalid = function(uid, protoId)
       return ("Invalid Item Id found. Item No:%s, Id:%s has been removed from your inventory.")
          :format(uid, protoId)
    end,
}
