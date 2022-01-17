OpenNefia.Prototypes.Elona.God.Elona = {
    Ehekatl = {
        Name = "幸運のエヘカトル",
        ShortName = "エヘカトル",
        Desc = {
            Ability = "エヘカトル流魔術(自動:マナの消費がランダムになる)",
            Bonus = "魅力 / 運 / 回避 / 魔力の限界 / 釣り/ 鍵開け",
            Offering = "死体 / 魚",
            Text = "エヘカトルは幸運の女神です。エヘカトルを信仰した者は、運を味方につけます。",
        },
        Servant = "この猫に舐められた武具は、エヘカトルの祝福を授かるようだ。祝福を受けた武具にはエンチャントが一つ付与される。",
        Talk = {
            Believe = "「わ〜ほんとに信じてくれるの？くれるの？」",
            Betray = "「うみみやゅっ！裏切っちゃうの？ちゃうの？」",
            FailToTakeOver = "「バカバカバカ！バカ！」",
            Kill = {
                "「もっと！もっと！」",
                "「死んじゃったよ！たよ！」",
                "「うっみゅーうみゅうみゅ」",
            },
            Night = "「寝るの？本当に寝るの？おやつみ！」",
            Offer = "「うみみゃぁ！」",
            Random = "「たらばがに！」",
            ReadyToReceiveGift = "「どきどきどき。君のこと、好きだよ。だよ！」",
            ReadyToReceiveGift2 = "「好き！好き好き好きっ！だいすき！君とは死ぬまでずっと一緒だよ。だよ！」",
            ReceiveGift = "「これあげるぅ…大切にしてね！…してね！」",
            TakeOver = "「わおっ♪嬉しい！好き！大好き！」",
            Welcome = "「みゃみゃ？帰って来たの？来たの？たくさん待ってたよ！」",
            WishSummon = "「うみみゅみゅぁ！」",
        },
    },

    Itzpalt = {
        Name = "元素のイツパロトル",
        ShortName = "イツパロトル",
        Desc = {
            Ability = "魔力の吸収(スキル:周囲の空気からマナを吸い出す)",
            Bonus = "魔力 / 瞑想 / 火炎耐性 / 冷気耐性 / 電撃耐性",
            Offering = "死体 / 杖",
            Text = "イツパロトルは元素を司る神です。イツパロトルを信仰した者は、魔力を大気から吸収し、元素に対する保護を受けることができます。",
        },
        Servant = "この追放者は連続魔法を使えるようだ。",
        Talk = {
            Believe = "「我が期待に応えて見せよ」",
            Betray = "「我を裏切ると？愚かなり」",
            FailToTakeOver = "「汝の愚かな試みの代償を味わうがよい」",
            Kill = {
                "「なかなかのものだ」",
                "「そして魂は元素へと還る」",
                "「高らかに我が名を唱えよ。屍に炎と安息を」",
            },
            Night = "「旅の疲れを癒すがよい。我が費えることなき紅蓮の炎が、夜のとばりに紛れる邪なる者から汝を守るであろう」",
            Offer = "「汝の贈り物に感謝しよう」",
            Random = {
                "「汝油断することなかれ」",
                "「汝油断することなかれ」",
                "「我々の抱える痛みを、定命の者が理解することはないであろう」",
                "「我々の抱える痛みを、定命の者が理解することはないであろう」",
                "「我が名はイツパロトル。元素の起源にて、最古の炎を従えし王、全ての神の主なり」",
                "「我が名はイツパロトル。元素の起源にて、最古の炎を従えし王、全ての神の主なり」",
                "「神々の戦いに終わりはない。来るべき時は、汝も我が軍門の元で働いてもらうであろう」",
                "「神々の戦いに終わりはない。来るべき時は、汝も我が軍門の元で働いてもらうであろう」",
                "「神々の戦いに終わりはない。来るべき時は、汝も我が軍門の元で働いてもらうであろう」",
            },
            ReadyToReceiveGift = "「我が名を語るに相応しい者が絶えて久しい。汝ならば、あるいは…」",
            ReadyToReceiveGift2 = "「見事なり、定命の者よ。汝を、我が信頼に値する唯一の存在として認めよう」",
            ReceiveGift = "「汝の忠誠に答え、贈り物を授けよう」",
            TakeOver = "「良し。汝の行いは覚えておこう」",
            Welcome = "「定命の者よ、よくぞ戻ってきた」",
            WishSummon = nil, -- TODO No summon for now.
        },
    },

    Jure = {
        Name = "癒しのジュア",
        ShortName = "ジュア",
        Desc = {
            Ability = "ジュアの祈り(スキル:失った体力を回復)",
            Bonus = "意思 / 治癒 / 瞑想 / 解剖学 / 料理 / 魔道具 / 魔力の限界",
            Offering = "死体 / 鉱石",
            Text = "ジュアは癒しの女神です。ジュアを信仰した者は、傷ついた身体を癒すことができます。",
        },
        Servant = "この防衛者は致死ダメージを受けた仲間をレイハンドで回復できるようだ。レイハンドは眠るたびに再使用可能になる。",
        Talk = {
            Believe = "「べ、別にアンタの活躍なんて期待してないんだからねっ」",
            Betray = "「な、何よ。アンタなんか、いなくたって寂しくないんだからね！」",
            FailToTakeOver = "「な、なにするのよ！」",
            Kill = {
                "「や、やるじゃない」",
                "「や、やめてよ。気持ち悪い」",
                "「こっち来ないで！」",
            },
            Night = "「お、おやすみのキスなんて…絶対にやだからね！」",
            Offer = "「べ、別に嬉しくなんかないんだからねっ！」",
            Random = { "「な、なによバカ！」", "「私はこの仕事に向いているのかなあ」" },
            ReadyToReceiveGift = "「や、やめてよ。アンタのことなんか、大好きじゃないんだからねっ。バカ！」",
            ReadyToReceiveGift2 = "「べ、別にアンタのこと愛してなんかいないんだからっ。あたしの傍から離れたらしょうちしないからねっ！このバカぁ…！",
            ReceiveGift = "「ア、アンタのためにしてあげるんじゃないんだからっ」",
            TakeOver = "「な、なによ。ほ、褒めてなんかあげないわよ！」",
            Welcome = "「べ、別にアンタが帰ってくるのを待ってたんじゃないからね！」",
            WishSummon = nil, -- TODO No summon for now.
        },
    },

    Kumiromi = {
        Name = "収穫のクミロミ",
        ShortName = "クミロミ",
        Desc = {
            Ability = "生命の輪廻(自動：腐った作物から種を取り出す)",
            Bonus = "感覚 / 器用 / 習得 / 栽培 / 錬金術 / 裁縫 / 読書",
            Offering = "死体 / 野菜 / 種",
            Text = "クミロミは収穫の神です。クミロミを信仰した者は、大地の恵みを収穫し、それを加工する術を知ります。",
        },
        Servant = "この妖精は食後に種を吐き出すようだ。",
        Talk = {
            Believe = "「よくきたね…期待…しているよ」",
            Betray = "「裏切り…許さない…」",
            FailToTakeOver = "「敵には…容赦しない…絶対」",
            Kill = {
                "「汚れたね」",
                "「ほどほどに」",
                "「…これが…君の望んでいた結果？」",
            },
            Night = "「おやすみ…明日…また朗らかな芽を吹いておくれ」",
            Offer = "「ありがたい…とてもいいよ…これ」",
            Random = {
                "「木々のさえずり…森の命が奏でる歌…耳をすませば…」",
                "「争いごとは…醜い」",
                "「僕のエヘカトル…今はもう…かつての面影さえない」",
                "「僕は与えよう…君たちが奪う以上のものを」",
            },
            ReadyToReceiveGift = "「君は…大切なしもべだ…」",
            ReadyToReceiveGift2 = "「ずっと一緒…だよね？…もう離さない…君が死ぬまで」",
            ReceiveGift = "「いいもの…あげる…」",
            TakeOver = "「よくやった…ほんとに…」",
            Welcome = "「おかえり…待っていた」",
            WishSummon = "工事中。",
        },
    },

    Lulwy = {
        Name = "風のルルウィ",
        ShortName = "ルルウィ",
        Desc = {
            Ability = "ルルウィの憑依(スキル:瞬間的に高速になる)",
            Bonus = "感覚 / 速度 / 弓 / クロスボウ / 隠密 / 魔道具",
            Offering = "死体 / 弓",
            Text = "ルルウィは風を司る女神です。ルルウィを信仰した者は、風の恩恵を受け素早く動くことが可能になります。",
        },
        Servant = "この黒天使はブーストした時に恐るべき力を発揮するようだ。",
        Talk = {
            Believe = "「私を選んだのは正解よ。たっぷり可愛がってあげるわ、子猫ちゃん」",
            Betray = "「アハハ。馬鹿ね。私なしで生きていくの？」",
            FailToTakeOver = "「やってくれたわね、ゴミの分際で。お仕置きよ」",
            Kill = {
                "「不潔ね。血を拭いなさいよ」",
                "「アハハ！ミンチミンチィ！」",
                "「まあ、いけない子猫ちゃん」",
            },
            Night = "「いいわ、少しの間だけ首枷を外してあげる。存分に休息を楽しみなさい」",
            Offer = "「あら、気の利いたものをくれるわね。下心でもあるの？」",
            Random = {
                "「みじめなブタども」",
                "「マニ？その名を再び口にしたらミンチよ、子猫ちゃん」",
                "「前の下僕は、八つ裂きにしてシルフ達の餌にしたわ。髪型がちょっと気に食わなかったから。アハハ！」",
                "「私の子供達は風の声、何事にも縛られてはいけない。オマエもよ」",
            },
            ReadyToReceiveGift = "「どこまでも私のために尽くしなさい。オマエは私の一番の奴隷なんだから」",
            ReadyToReceiveGift2 = "「私に従え。全てを委ねろ。オマエの綺麗な顔を傷つけるブタどもは、私がミンチにしてあげるわ」",
            ReceiveGift = "「下僕のオマエにご褒美よ。大事に使いなさい。」",
            TakeOver = "「褒めてあげるわ。私の可愛い小さなお人形さん」",
            Welcome = "「どこホッツキ歩いてたのよ。もっと調教が必要ね」",
            WishSummon = "「アタシを呼びつけるとは生意気ね。」",
        },
    },

    Mani = {
        Name = "機械のマニ",
        ShortName = "マニ",
        Desc = {
            Ability = "マニの分解術(自動:罠からマテリアルを取り出す)",
            Bonus = "器用 / 感覚 / 銃 / 治癒 / 探知 / 宝石細工 / 鍵開け / 大工",
            Offering = "死体 / 銃器 / 機械",
            Text = "マニは機械仕掛けの神です。マニを信仰した者は、機械や罠に対する膨大な知識を得、またそれらを効果的に利用する術を知ります。",
        },
        Servant = "このアンドロイドはブーストした時に恐るべき力を発揮するようだ。",
        Talk = {
            Believe = "「入信者か。私の名を貶めないよう励むがいい」",
            Betray = "「やってくれたな。裏切り者め！」",
            FailToTakeOver = "「馬鹿にしてくれたじゃないか」",
            Kill = {
                "「いいぞ」",
                "「分解してみたくならないか？」",
                "「その魂なら質の良い機械が組めそうだ」",
            },
            Night = "「短い命の多くを無駄な眠りに費やすとは、生身の体とは不自由なものだ。だが今はそう、休むがいい。また私に仕えるために」",
            Offer = "「なかなかの贈り物だ」",
            Random = {
                "「お前も体を機械化したらどうだ？」",
                "「常に、私の名に恥じぬよう振舞え」",
                "「焦るな。すぐに機械が全てを支配する時代が来る」",
            },
            ReadyToReceiveGift = "「お前はまさに信者の模範だな」",
            ReadyToReceiveGift2 = "「お前は最愛のシモベだ。その魂を私に捧げろ。お前を必ず守ってみせよう」",
            ReceiveGift = "「お前は忠実なるシモベだ。これを有効に使うがいい」",
            TakeOver = "「やるじゃないか。見直したよ」",
            Welcome = "「戻ってきたか。案外ホネのある奴だな」",
            WishSummon = "工事中。",
        },
    },

    Opatos = {
        Name = "地のオパートス",
        ShortName = "オパートス",
        Desc = {
            Ability = "オパートスの甲殻(自動:受ける物理ダメージを減らす)",
            Bonus = "筋力 / 耐久 / 盾 / 重量挙げ / 採掘 / 魔道具",
            Offering = "死体 / 鉱石",
            Text = "オパートスは大地の神です。オパートスを信仰した者は、高い防御力と破壊力を身につけます。",
        },
        Servant = "この騎士はある程度重いものをもたせても文句をいわないようだ。",
        Talk = {
            Believe = "「フハッハハハハ。逃がさんぞ！」",
            Betray = "「フフッフフッフハハハハハ！」",
            FailToTakeOver = "「フハハハハッ、弱い弱い」",
            Kill = { "「フハハハ！」", "「逝け！逝け！フハハハッ！」", "「フハーン！」" },
            Night = "「フハハハハ。付いて行くぞぉ、夢の中までも」",
            Offer = "「フッハッハハー！」",
            Random = "「フハハハハ」",
            ReadyToReceiveGift = "「フハァッハハハハハハ！愉快愉快！」",
            ReadyToReceiveGift2 = "「ファハハハハハハハハハハハハハー！フワハァー！」",
            ReceiveGift = "「フハハハァ！受け取れィ」",
            TakeOver = "「フハハッ、いいぞいいぞ」",
            Welcome = "「フハハハハハ！！おかえり」",
            WishSummon = "工事中。",
        },
    },
}