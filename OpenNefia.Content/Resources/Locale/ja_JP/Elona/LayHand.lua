Elona.LayHand = {
    Dialog = function(healer)
        return ("%sは叫んだ。「この者にジュアの加護を。レイハンド！」"):format(_.name(healer))
    end,
    IsHealed = function(target)
        return ("%sは回復した。"):format(_.name(target))
    end,
}
