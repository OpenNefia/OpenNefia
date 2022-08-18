OpenNefia.Prototypes.Entity.Elona = {
    AreaVernis = {
        MetaData = {
            Name = "ヴェルニース",
        },
        AreaEntrance = {
            EntranceMessage = "ヴェルニースの街が見える。辺りは活気に満ちている。",
        },

        VillagerTalk = {
            "ヴェルニースへようこそ。",
            function(npc)
                return ("鉱山のおかげでこの街は潤っている%s。"):format(_.noda(npc))
            end,
            function(npc)
                return ("ヴェルニースは歴史ある炭鉱街%s。"):format(_.da(npc))
            end,
            function(npc)
                return ("あのバーでピアノを弾くのは、やめたほうがいい%s。"):format(_.yo(npc))
            end,
            function(npc)
                return ("イェルスとエウダーナの戦争に巻き込まれるのは、ごめん%s。"):format(
                    _.da(npc)
                )
            end,
            function(npc)
                return ("レシマスで何かが見つかったらしい%s。"):format(_.yo(npc))
            end,
            function(npc)
                return ("新たなスキルを得る方法があるらしい%s。"):format(_.yo(npc))
            end,
            function(npc)
                return ("荷物の持ち過ぎには、注意したほうがいい%s。"):format(_.na(npc))
            end,
            function(npc)
                return ("お墓の傍にいる人？ああ、あの薬中とは関わっちゃいけない%s。"):format(
                    _.yo(npc)
                )
            end,
            function(npc)
                return ("ミシェスはぬいぐるみマニア%s。"):format(_.da(npc))
            end,
            function(npc)
                return ("王都パルミアまでは、道をひたすら東にすすめばいい%s。"):format(
                    _.yo(npc)
                )
            end,
            function(npc)
                return ("シーナの尻は最高%s。"):format(_.da(npc))
            end,
            function(npc)
                return ("最近は、盗賊団なる輩がいて困る%s。"):format(_.yo(npc))
            end,
        },
    },

    AreaYowyn = {
        MetaData = {
            Name = "ヨウィン",
        },
        AreaEntrance = {
            EntranceMessage = "ヨウィンの村が見える。懐かしい土の匂いがする。",
        },

        VillagerTalk = {
            function(npc)
                return ("こんな田舎街にもちゃんとヨウィンと言う名前があるん%s。"):format(
                    _.da(npc)
                )
            end,
            function(npc)
                return ("馬ならここで買って行くと良い%s。"):format(_.yo(npc))
            end,
            function(npc)
                return ("収穫期はいつも人が足りない%s。"):format(_.na(npc))
            end,
            function(npc)
                return ("何もない場所だけど、ゆっくりしていくといい%s。"):format(_.yo(npc))
            end,
            function(npc)
                return ("西に無法者の街があるそう%s。"):format(_.da(npc))
            end,
            function(npc)
                return ("街を出て東の道沿いに行けば王都%s。"):format(_.da(npc))
            end,
            function(npc)
                return ("南西に古い城があるのを見かけた人がいる%s。"):format(_.noda(npc))
            end,
            function(npc)
                return ("この街の葬具は他に自慢出来る一品%s。"):format(_.da(npc))
            end,
        },
    },

    AreaPalmia = {
        MetaData = {
            Name = "パルミア",
        },
        AreaEntrance = {
            EntranceMessage = "パルミアの都がある。都は高い壁に囲われている。",
        },

        VillagerTalk = {
            "パルミア国の王都へようこそ。",
            function(npc)
                return ("ミーアのしゃべり方はどうにからならないか%s。"):format(_.na(npc))
            end,
            function(npc)
                return (
                    "パルミアは何でも揃っていて便利でいい%s、広いから人探しが大変%s。"
                ):format(_.ga(npc), _.da(npc))
            end,
            function(npc)
                return ("ジャビ様とスターシャ様は、本当に仲がいい%s。"):format(_.noda(npc))
            end,
            function(npc)
                return ("パルミア名産といえば、貴族のおもちゃ%s。"):format(_.da(npc))
            end,
        },
    },

    AreaDerphy = {
        MetaData = {
            Name = "ダルフィ",
        },
        AreaEntrance = {
            EntranceMessage = "ダルフィの街がある。何やら危険な香りがする。",
        },

        VillagerTalk = {
            "無法者の街、ダルフィへようこそ。",
            function(npc)
                return ("ノエルみたいにはなれない%s。"):format(_.na(npc))
            end,
            function(npc)
                return ("その道に興味があるなら盗賊ギルドに行くと良い%s。"):format(_.yo(npc))
            end,
            function(npc)
                return ("奴隷は世に必要なもの%s。"):format(_.da(npc))
            end,
            function(npc)
                return ("アリーナで血を見るのが好き%s。"):format(_.da(npc))
            end,
            function(npc)
                return ("この街にはガードがいない%s。"):format(_.yo(npc))
            end,
        },
    },

    AreaPortKapul = {
        MetaData = {
            Name = "ポート・カプール",
        },
        AreaEntrance = {
            EntranceMessage = "ポート・カプールが見える。港は船で賑わっている。",
        },

        VillagerTalk = {
            function(npc)
                return ("潮風が香る%s。"):format(_.na(npc))
            end,
            function(npc)
                return ("ペットアリーナで観戦するのが趣味%s。"):format(_.da(npc))
            end,
            function(npc)
                return ("ラファエロは女の敵%s。"):format(_.dana(npc))
            end,
            function(npc)
                return ("もっと強くなりたいのなら戦士ギルドに行くと良い%s。"):format(_.yo(npc))
            end,
            function(npc)
                return ("ここの海産物は内陸部で高く売れる%s。"):format(_.noda(npc))
            end,
            function(npc)
                return ("ピラミッドにはどうやったら入れる%s？"):format(_.noda(npc))
            end,
        },
    },

    AreaNoyel = {
        MetaData = {
            Name = "ノイエル",
        },
        AreaEntrance = {
            EntranceMessage = "ノイエルの村がある。子供たちの笑い声が聞こえる。",
        },

        VillagerTalk = {
            function(npc)
                return ("えっくし！ うぅ、今日も寒い%s。"):format(_.na(npc))
            end,
            function(npc)
                return ("毎日雪かきが大変%s。"):format(_.da(npc))
            end,
            function(npc)
                return ("あの巨人の名前はエボンと言うそう%s。"):format(_.da(npc))
            end,
            function(npc)
                return ("罪悪感に耐え切れなくなったら教会に行くと良い%s。"):format(_.yo(npc))
            end,
            "寒いっ！",
            function(npc)
                return ("少し南に行った所に小さな工房が建ってるのを見た%s。"):format(
                    _.noda(npc)
                )
            end,
        },
    },

    AreaLumiest = {
        MetaData = {
            Name = "ルミエスト",
        },
        AreaEntrance = {
            EntranceMessage = "ルミエストの都が見える。水のせせらぎが聴こえる。",
        },

        VillagerTalk = {
            "ようこそ、水と芸術の街へ。",
            function(npc)
                return ("どこかに温泉で有名な街があるそう%s。"):format(_.da(npc))
            end,
            function(npc)
                return ("この街じゃ、どこでも釣りが出来る%s。"):format(_.noda(npc))
            end,
            function(npc)
                return ("絵画に関してはうるさい%s。"):format(_.yo(npc))
            end,
            function(npc)
                return ("パルミアには、魔術師ギルドはこの街にしかない%s。"):format(_.noda(npc))
            end,
        },
    },

    AreaShelter = {
        VillagerTalk = {
            function(npc)
                return ("はやく天候が回復するといい%s。"):format(_.na(npc))
            end,
            function(npc)
                return ("このシェルターは、みんなの献金で建設した%s。"):format(_.noda(npc))
            end,
            function(npc)
                return ("シェルターがあって助かる%s。"):format(_.yo(npc))
            end,
            function(npc)
                return ("ものすごく暇%s。"):format(_.da(npc))
            end,
            function(npc)
                return ("なんだかワクワクする%s。"):format(_.yo(npc))
            end,
            function(npc)
                return ("食料は十分あるみたい%s。"):format(_.da(npc))
            end,
        },
    },
}
