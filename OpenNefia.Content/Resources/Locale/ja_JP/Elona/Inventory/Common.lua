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

    SomethingFalls = {
        FromBackpack = function(item, owner)
            return ("%sが%sから地面に落ちた。"):format(_.name(item), _.name(owner))
        end,
        AndDisappears = "何かが地面に落ちて消えた…",
    },

    NameModifiers = {
        Ground = "(足元)",
    },
}
