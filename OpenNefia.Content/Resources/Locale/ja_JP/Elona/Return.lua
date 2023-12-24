Elona.Return = {
    Prompt = "どの場所に帰還する？",
    LevelCounter = function(level)
        return ("%s階"):format(level)
    end,
    NoLocations = "この大陸には帰還できる場所が無い。",
    Begin = "周囲の大気がざわめきだした。",
    Cancel = function(source)
        return ("%s帰還を中止した。"):format(_.sore_wa(source))
    end,
    Prevented = "不思議な力が帰還を阻止した。",

    Result = {
        AllyPrevents = "今は帰還できない仲間を連れている。",
        Overburdened = "どこからか声が聞こえた。「悪いが重量オーバーだ」",
        CargoOverburdened = "荷車が重すぎて帰還できなかった。",
        CommitCrime = "あなたは法を犯した。",
        DimensionalDoorOpens = "あなたは次元の扉を開けた。",
        SentToJail = "気まぐれな時の管理者により次元は歪められた！",
    },
}
