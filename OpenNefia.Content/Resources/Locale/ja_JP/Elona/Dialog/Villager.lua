Elona.Dialog.Villager = {
    Choices = {
        Talk = "話がしたい",
        Trade = "アイテム交換",
        Sex = "気持ちいいことしない？",
    },

    Talk = {
        Default = {
            function(npc)
                return ("%s？ …なんだか聞き覚えが%s。"):format(_.alias(_.player()), _.aru(npc))
            end,
            function(npc)
                return ("暇%s。"):format(_.da(npc))
            end,
            function(npc)
                return ("ん？ 何か用%s？"):format(_.kana(npc))
            end,
            function(npc)
                return ("たまには豪勢な食事をしたい%s。"):format(_.na(npc))
            end,
            function(npc)
                return ("神の存在なくしてこの世は無意味%s！"):format(_.da(npc))
            end,
            function(npc)
                return (
                    "エーテルの風は異形の森のせいで発生しているとか…ま、あまり興味ない%s。"
                ):format(_.na(npc))
            end,
        },

        Ally = {
            "(あなたをじっと見ている)",
            "…？",
        },

        Prostitute = {
            function(npc)
                return ("あ〜ら、あなたいい%sね。一晩の夢を見させてあげてもいいのよ。"):format(
                    _.loc("Elona.Gender.Names." .. _.gender(_.player()) .. ".Informal")
                )
            end,
        },

        Bored = {
            "(退屈そうにあなたを見ている)",
            function(npc)
                return ("(%sはあなたを一瞥すると、そっぽを向いた。)"):format(_.name(npc))
            end,
        },

        ChristmasFestival = {
            function(npc)
                return ("ようこそ、ノイエルの聖夜祭へ！楽しんでいって%s。"):format(_.kure(npc))
            end,
            function(npc)
                return (
                    "聖夜祭は、聖ジュア祭とも呼ばれてい%s。癒しの女神を称え、年の終わりを祝う宴でもある%s。"
                ):format(_.ru(npc), _.noda(npc))
            end,
            function(npc)
                return (
                    "モイアーのやつ、今年の聖夜祭ではなにか特別な品物を用意すると張り切っていた%s。"
                ):format(_.na(npc))
            end,
            function(npc)
                return (
                    "%s、あの神々しいジュア像をもう見た%s？なんとも美しく可憐じゃない%s！%sもこの機に改宗するといい%s！"
                ):format(_.kimi(npc), _.kana(npc), _.ka(npc), _.kimi(npc), _.yo(npc))
            end,
            function(npc)
                return ("今宵は愛する人と気持ちいいことをする%s！"):format(_.noda(npc))
            end,
            function(npc)
                return (
                    "この祭りを見るために、この時期には多くの観光客がノイエルを訪れる%s。"
                ):format(_.noda(npc))
            end,
            function(npc)
                return (
                    "聖夜祭は歴史ある祭り%s。かつてはパルミアの王室もジュア像に祈りを捧げに来ていたらしい%s。"
                ):format(_.da(npc), _.yo(npc))
            end,
            function(npc)
                return (
                    "伝説では、ジュアの癒しの涙は、いかなる難病をも治療するといわれてい%s。事実、聖夜祭の期間には、盲人が光を取り戻すなど、数々の奇跡が目撃されている%s。"
                ):format(_.ru(npc), _.noda(npc))
            end,
        },

        Moyer = {
            "お客さん、怯えなくても大丈夫だ。この怪物は、魔法によって身動き一つとれないんだ。さあ、見物ついでに、うちの商品も見て言ってくれ。",
            "これなるは、かのネフィアの迷宮ベロンを支配した、伝説の火の巨人！今日立ち寄ったあなたは、実に運がいい！さあ、この神々しくも禍々しい怪物の雄姿を、とくとご覧あれ。商品を買ってくれれば、お触りもオッケーだよ。",
        },

        Personality = {
            ["0"] = {
                function(npc)
                    return ("ジャビ王は聡明なお方%s。"):format(_.da(npc))
                end,
                function(npc)
                    return ("クリムエールをふらふらになるまで飲みたい%s。"):format(_.na(npc))
                end,
                function(npc)
                    return ("猫は何故こんなにかわいいの%s。"):format(_.kana(npc))
                end,
                function(npc)
                    return ("イェルスは最近台頭してきた新王国%s。"):format(_.da(npc))
                end,
                function(npc)
                    return (
                        "エウダーナは、他のどの国よりも先に、ネフィアの探索と研究をはじめた%s。"
                    ):format(_.noda(npc))
                end,
                function(npc)
                    return ("店の主人は不死身なの%s。"):format(_.kana(npc))
                end,
                function(npc)
                    return ("どうやら、アーティファクトの所在を知る方法があるみたい%s。"):format(
                        _.da(npc)
                    )
                end,
                function(npc)
                    return ("大食いトドは魚が好きみたい%s。"):format(_.da(npc))
                end,
                function(npc)
                    return (
                        "病気や体調が悪い時は、十分に睡眠をとって休むのが大事%s。祝福された回復のポーションも病気に効くと、昔からよくいわれてい%s。"
                    ):format(_.da(npc), _.ru(npc))
                end,
            },
            ["1"] = {
                function(npc)
                    return ("ともかく、世の中お金が大事%s。"):format(_.da(npc))
                end,
                function(npc)
                    return ("どこかにお金が落ちていない%s。"):format(_.kana(npc))
                end,
                function(npc)
                    return ("経済の話題には関心がある%s。"):format(_.noda(npc))
                end,
                function(npc)
                    return ("プラチナ硬貨はなかなか手に入らない%s。"):format(_.na(npc))
                end,
                function(npc)
                    return ("エウダーナの財政は、少し苦しいらしい%s。"):format(_.yo(npc))
                end,
                function(npc)
                    return (
                        "戦後のザナンは形式こそ王国制をとるものの、実質的にはエイス・テールの諸国をモデルにした経済国家%s。"
                    ):format(_.da(npc))
                end,
            },
            ["2"] = {
                function(npc)
                    return (
                        "ノースティリス西部には、数々の遺跡やダンジョンがあり、ネフィアとよばれている%s。"
                    ):format(_.noda(npc))
                end,
                function(npc)
                    return ("エイス・テールは第七期の文明%s。高度な科学を持っていた%s。"):format(
                        _.da(npc),
                        _.noda(npc)
                    )
                end,
                function(npc)
                    return (
                        "エイス・テールは、魔法と科学を対立するものと考えていたよう%s。"
                    ):format(_.da(npc))
                end,
                function(npc)
                    return ("科学について語るのが好き%s。"):format(_.da(npc))
                end,
                function(npc)
                    return ("ヴェルニースはパルミアで一番大きな炭鉱街%s。"):format(_.da(npc))
                end,
                function(npc)
                    return ("ルミエストは芸術の街として有名%s。"):format(_.dana(npc))
                end,
            },
            ["3"] = {
                function(npc)
                    return ("食料の供給源は、しっかり決めておいたほうがいい%s。"):format(
                        _.yo(npc)
                    )
                end,
                function(npc)
                    return (
                        "まだ解き明かされない謎が多く眠るネフィアは、冒険者にとって聖地のようなもの%s。"
                    ):format(_.da(npc))
                end,
                function(npc)
                    return ("永遠の盟約…？そんな言葉は聞いたことがない%s。"):format(_.na(npc))
                end,
                function(npc)
                    return ("シエラ・テールは十一期目の文明%s。"):format(_.da(npc))
                end,
                function(npc)
                    return ("色々なところを旅するのが好き%s。"):format(_.da(npc))
                end,
                function(npc)
                    return (
                        "もし冒険が難しくなってきたら、名声を下げてみればいいかもしれない%s。"
                    ):format(_.na(npc))
                end,
                function(npc)
                    return ("宝くじ箱は鍵開けの訓練にもってこい%s。"):format(_.da(npc))
                end,
            },
        },

        Rumor = {
            function(npc)
                return ("うさぎの尻尾は幸運を呼ぶみたい%s。"):format(_.dana(npc))
            end,
            function(npc)
                return (
                    "乞食は体内を浄化する魔法のペンダントを持っていることがあるみたい%s。奴らは何でも食べるから%s。"
                ):format(_.da(npc), _.na(npc))
            end,
            function(npc)
                return ("ゾンビは稀に奇妙なポーションを落とすよう%s。"):format(_.da(npc))
            end,
            function(npc)
                return (
                    "そういえば、以前とてつもない名器を奏でる詩人の演奏を聴いたことが%s。感動して、つい、履いていた高価な靴を投げてしまった%s。"
                ):format(_.aru(npc), _.yo(npc))
            end,
            function(npc)
                return ("なんでもマミーは死者を蘇らせる書をもっているそう%s。"):format(
                    _.da(npc)
                )
            end,
            function(npc)
                return ("見てしまった%s！死刑執行人が、首を切られたのに生きかえるのを！"):format(
                    _.noda(npc)
                )
            end,
            function(npc)
                return (
                    "異形の目に見入られた者の肉体は変化するという%s、たまに生物の進化を促すポーションを落とすらしい%s。"
                ):format(_.ga(npc), _.yo(npc))
            end,
            function(npc)
                return ("妖精はとっても秘密な経験を隠しているらしい%s！"):format(_.yo(npc))
            end,
            function(npc)
                return ("ロックスロアーの投げる石をあなどってはいけない%s。"):format(_.yo(npc))
            end,
            function(npc)
                return (
                    "銀眼の魔女には気をつけるの%s。あの大剣に斬りつけられたらひとたまりもない%s。たまにとんでもない業物を持っていることもあるらしい%s。"
                ):format(_.dana(npc), _.daro(npc), _.ga(npc))
            end,
            function(npc)
                return ("キューピットが重そうなものを運んでいるのをみた%s。"):format(_.yo(npc))
            end,
            function(npc)
                return ("夢で神様に会える%s？"):format(_.kana(npc))
            end,
            function(npc)
                return (
                    "黄色い首輪をつけた四本腕の化け物に出会ったのなら、すぐに逃げるのが賢明%s"
                ):format(_.dana(npc))
            end,
            "盗賊団の殺し屋は、射撃の回数を増やす魔法の首輪を稀に所持しているらしい。",
            function(npc)
                return ("貴族の中には変わった物を収集している者がいるらしい%s。"):format(
                    _.ga(npc)
                )
            end,
            function(npc)
                return (
                    "パーティーで演奏していると、酔った客がたまに変なものを投げてくるの%s。"
                ):format(_.da(npc))
            end,
            function(npc)
                return ("赤い腰当をしたロボットを見たことがある%s。"):format(_.na(npc))
            end,
            function(npc)
                return (
                    "インプが持つ魔法の巻物は、アーティファクトの名前を変えられるそう%s。"
                ):format(_.dana(npc))
            end,
            function(npc)
                return (
                    "ヨウィンの無邪気な少女は、不思議な宝物を大切に持っているみたい%s。"
                ):format(_.da(npc))
            end,
            function(npc)
                return ("この前、とても綺麗な貝をかぶったやどかりを見た%s。"):format(_.yo(npc))
            end,
            function(npc)
                return ("盗賊団の連中は、何やら怪しい薬を常用しているらしい%s。"):format(
                    _.na(npc)
                )
            end,
        },

        Shopkeeper = {
            function(npc)
                return ("店の経営は、なかなか難しい%s。"):format(_.na(npc))
            end,
            function(npc)
                return ("他の店では、足元を見られないよう、気をつけたほうがいい%s。"):format(
                    _.yo(npc)
                )
            end,
            function(npc)
                return ("ごろつきを追い払えるぐらい強くないと、店主はつとまらない%s。"):format(
                    _.yo(npc)
                )
            end,
            function(npc)
                return ("何かの時のために、店を継ぐ人は決めて%s。"):format(_.ru(npc))
            end,
            function(npc)
                return ("いらっしゃい。ゆっくり見ていって%s。"):format(_.kure(npc))
            end,
            function(npc)
                return ("品揃えには自信がある%s。"):format(_.noda(npc))
            end,
            function(npc)
                return ("さあ、自慢の商品を見ていって%s。"):format(_.kure(npc))
            end,
            function(npc)
                return ("最近は物騒な人が多くて大変%s。"):format(_.da(npc))
            end,
            function(npc)
                return (
                    "武器と防具はきちんと鑑定されたものでないと、高くは買い取れない%s。"
                ):format(_.yo(npc))
            end,
        },

        Slavekeeper = {
            function(npc)
                return ("お客さんも悪い人間%s。"):format(_.dana(npc))
            end,
            function(npc)
                return ("ひひひ…旦那も好き者%s。"):format(_.dana(npc))
            end,
        },
    },
}
