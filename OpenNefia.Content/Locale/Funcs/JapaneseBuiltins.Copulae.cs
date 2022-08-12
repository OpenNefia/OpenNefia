using OpenNefia.Content.Charas;
using OpenNefia.Content.Dialog;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Visibility;
using OpenNefia.Content.World;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Locale.Funcs
{
    public static partial class JapaneseBuiltins
    {
        private static string PrintCopula(string copulaName, object? obj, int? mark)
        {
            if (obj is not EntityUid uid)
                return "";

            var entMan = IoCManager.Resolve<IEntityManager>();
            var rand = IoCManager.Resolve<IRandom>();

            var copulaMark = Core.Utility.EnumHelpers.Clamp((CopulaMark)(mark ?? 0), CopulaMark.Period, CopulaMark.None);
            var gender = entMan.GetComponentOrNull<CharaComponent>(uid)?.Gender ?? Gender.Male;
            var copulaType = entMan.GetComponentOrNull<DialogComponent>(uid)?.CopulaType ?? CopulaType.Desu;

            var choices = CopulaTable[copulaName][copulaType][gender];

            return $"{rand.Pick(choices)}{CopulaMarks[copulaMark]}";
        }

        public sealed class CopulaEntry
        {
            private Dictionary<Gender, string[]> Copulas = new();

            public CopulaEntry(string[] male, string[] female)
            {
                Copulas[Gender.Male] = male;
                Copulas[Gender.Female] = female;
            }

            public string[] this[Gender gender]
            {
                get => Copulas[gender];
            }
        }

        private enum CopulaMark : int
        {
            Period,
            QuestionMark,
            ExclamationMark,
            None
        }

        private static Dictionary<CopulaMark, string> CopulaMarks = new()
        {
            { CopulaMark.Period, "﻿。" },
            { CopulaMark.QuestionMark, "﻿﻿？" },
            { CopulaMark.ExclamationMark, "﻿！" },
            { CopulaMark.None, "﻿" },
        };

#pragma warning disable format

        private static Dictionary<string, Dictionary<CopulaType, CopulaEntry>> CopulaTable = new()
        {
            {
                "yoro",
                 new() {
                      { CopulaType.Desu,     new(male: new[] { "よろしくお願いします", "どうぞ、よろしくです" },
                                               female: new[] { "よろしくお願いしますわ", "よろしくです" })},
                      { CopulaType.Daze,     new(male: new[] { "よろしく頼むぜ", "よろしくな" },
                                               female: new[] { "よろしくね", "よろしくな" })},
                      { CopulaType.Dayo,     new(male: new[] { "よろしくね", "よろしくお願いするよ" },
                                               female: new[] { "よろしくねっ", "よろしく〜" })},
                      { CopulaType.Da,       new(male: new[] { "よろしく…", "今後とも、よろしく…" },
                                               female: new[] { "よろしくね…", "よろ…" })},
                      { CopulaType.Ja,       new(male: new[] { "よろしく頼もう", "よろしく頼むぞよ" },
                                               female: new[] { "よろしく頼むぞよ", "よろしく頼むぞな" })},
                      { CopulaType.DeGozaru, new(male: new[] { "よしなに", "よろしく頼むでござる" },
                                               female: new[] { "よろしくでござりまする", "どうぞよしなに" })},
                      { CopulaType.Ssu,      new(male: new[] { "よろしくッス" },
                                               female: new[] { "よろしくにゃの" })},
                }
            },
            {
                "dozo",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "はい、どうぞ", "お待ちどうさまです" },
                                               female: new[] { "はい、どうぞ", "注文の品ですわ" })},
                      { CopulaType.Daze,     new(male: new[] { "ほらよ", "ほれ" },
                                               female: new[] { "ほら", "待たせたね" })},
                      { CopulaType.Dayo,     new(male: new[] { "はい、お待ち", "さあ、どうぞ" },
                                               female: new[] { "さあ、どうぞ", "お待ちどうさま" })},
                      { CopulaType.Da,       new(male: new[] { "ほら…", "待たせたな…" },
                                               female: new[] { "はい…", "どうぞ…" })},
                      { CopulaType.Ja,       new(male: new[] { "ほうれ", "ほれ、受け取りたまえ" },
                                               female: new[] { "ほれ、受け取るが良い", "ほれ、待たせたのう" })},
                      { CopulaType.DeGozaru, new(male: new[] { "お待たせ申した", "待たせたでござる" },
                                               female: new[] { "お待たせ致しました", "ささ、どうぞ" })},
                      { CopulaType.Ssu,      new(male: new[] { "お待たせッス" },
                                               female: new[] { "お待たせにゃん" })},
                }
            },
            {
                "thanks",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "感謝します", "ありがとうございます" },
                                               female: new[] { "感謝します", "ありがとうございます" })},
                      { CopulaType.Daze,     new(male: new[] { "ありがとよ", "ありがたい" },
                                               female: new[] { "礼を言うよ", "ありがたいね" })},
                      { CopulaType.Dayo,     new(male: new[] { "ありがとう", "感謝するよ" },
                                               female: new[] { "ありがとう〜", "感謝するわ" })},
                      { CopulaType.Da,       new(male: new[] { "礼を言う…", "感謝する…" },
                                               female: new[] { "ありがと…", "礼を言うわ…" })},
                      { CopulaType.Ja,       new(male: new[] { "礼を申すぞ", "感謝してつかわす" },
                                               female: new[] { "くるしゅうない", "礼をいってつかわす" })},
                      { CopulaType.DeGozaru, new(male: new[] { "かたじけない", "恩に着る" },
                                               female: new[] { "ありがたや", "お礼申し上げます" })},
                      { CopulaType.Ssu,      new(male: new[] { "アザーッス" },
                                               female: new[] { "にゃりーん" })},
                }
            },
            {
                "rob",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "悪いことは言わない。おやめなさい", "止めてください。きっと後悔しますよ" },
                                               female: new[] { "止めてくださいませ", "こういう時のために、傭兵に金をかけているのです" })},
                      { CopulaType.Daze,     new(male: new[] { "なんだ、貴様賊だったのか", "馬鹿な奴だ。後になって謝っても遅いぞ" },
                                               female: new[] { "ふん、返り討ちにしてくれるよ", "ごろつき風情に何ができる" })},
                      { CopulaType.Dayo,     new(male: new[] { "おい、傭兵さんたち、このごろつきを追い払ってくれ", "馬鹿な真似をするな。こっちには屈強の傭兵がいるんだぞ" },
                                               female: new[] { "やめて", "傭兵さんたち〜出番ですよ" })},
                      { CopulaType.Da,       new(male: new[] { "甘く見られたものだ…", "この護衛の数が見えないのか…" },
                                               female: new[] { "おやめ…", "愚かな試みよ…" })},
                      { CopulaType.Ja,       new(male: new[] { "なんたる無礼者か", "ほほほ、こやつめ" },
                                               female: new[] { "下賤の者どもの分際で", "ほほほ、殺してあげなさい" })},
                      { CopulaType.DeGozaru, new(male: new[] { "何をするでござるか" },
                                               female: new[] { "ご無体な", "まあ、お戯れが過ぎますわ" })},
                      { CopulaType.Ssu,      new(male: new[] { "見損なったッス" },
                                               female: new[] { "にゃりーん" })},
                }
            },
            {
                "ka",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "ですか" },
                                               female: new[] { "ですか" })},
                      { CopulaType.Daze,     new(male: new[] { "かよ", "か" },
                                               female: new[] { "かい" })},
                      { CopulaType.Dayo,     new(male: new[] { "かい", "なの" },
                                               female: new[] { "なの" })},
                      { CopulaType.Da,       new(male: new[] { "か…", "かよ…" },
                                               female: new[] { "なの…" })},
                      { CopulaType.Ja,       new(male: new[] { "かのう", "であるか" },
                                               female: new[] { "であるか" })},
                      { CopulaType.DeGozaru, new(male: new[] { "でござるか" },
                                               female: new[] { "でござりまするか" })},
                      { CopulaType.Ssu,      new(male: new[] { "ッスか" },
                                               female: new[] { "かにゃ", "かニャン" })},
                }
            },
            {
                "da",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "です", "ですね" },
                                               female: new[] { "ですわ", "です" })},
                      { CopulaType.Daze,     new(male: new[] { "だぜ", "だ" },
                                               female: new[] { "ね", "よ" })},
                      { CopulaType.Dayo,     new(male: new[] { "だよ" },
                                               female: new[] { "だわ", "よ" })},
                      { CopulaType.Da,       new(male: new[] { "だ…", "さ…" },
                                               female: new[] { "よ…", "ね…" })},
                      { CopulaType.Ja,       new(male: new[] { "じゃ", "でおじゃる" },
                                               female: new[] { "じゃ", "でおじゃるぞ" })},
                      { CopulaType.DeGozaru, new(male: new[] { "でござる", "でござるよ" },
                                               female: new[] { "でござりまする" })},
                      { CopulaType.Ssu,      new(male: new[] { "ッス" },
                                               female: new[] { "みゃん", "ミャ" })},
                }
            },
            {
                "noda",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "のです", "んです" },
                                               female: new[] { "のですわ", "のです" })},
                      { CopulaType.Daze,     new(male: new[] { "", "んだ" },
                                               female: new[] { "の" })},
                      { CopulaType.Dayo,     new(male: new[] { "んだよ", "んだ" },
                                               female: new[] { "わ", "のよ" })},
                      { CopulaType.Da,       new(male: new[] { "…", "んだ…" },
                                               female: new[] { "の…", "わ…" })},
                      { CopulaType.Ja,       new(male: new[] { "のじゃ", "のだぞよ" },
                                               female: new[] { "のじゃわ", "のだぞよ" })},
                      { CopulaType.DeGozaru, new(male: new[] { "のでござる" },
                                               female: new[] { "のでございます" })},
                      { CopulaType.Ssu,      new(male: new[] { "んだッス" },
                                               female: new[] { "のニャ", "のにゃん" })},
                }
            },
            {
                "noka",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "のですか", "んですか" },
                                               female: new[] { "のですか", "んですか" })},
                      { CopulaType.Daze,     new(male: new[] { "のか", "のだな" },
                                               female: new[] { "の", "のかい" })},
                      { CopulaType.Dayo,     new(male: new[] { "のかい", "の" },
                                               female: new[] { "の" })},
                      { CopulaType.Da,       new(male: new[] { "のか…" },
                                               female: new[] { "の…" })},
                      { CopulaType.Ja,       new(male: new[] { "のかのう", "のだな" },
                                               female: new[] { "のかね", "のだな" })},
                      { CopulaType.DeGozaru, new(male: new[] { "のでござるか" },
                                               female: new[] { "のでございます" })},
                      { CopulaType.Ssu,      new(male: new[] { "のッスか" },
                                               female: new[] { "にゃんか", "ニャン" })},
                }
            },
            {
                "kana",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "でしょうか", "ですか" },
                                               female: new[] { "かしら", "でしょう" })},
                      { CopulaType.Daze,     new(male: new[] { "か", "かい" },
                                               female: new[] { "か", "かい" })},
                      { CopulaType.Dayo,     new(male: new[] { "かな", "かなぁ" },
                                               female: new[] { "かな", "かなー" })},
                      { CopulaType.Da,       new(male: new[] { "かな…", "か…" },
                                               female: new[] { "かな…", "か…" })},
                      { CopulaType.Ja,       new(male: new[] { "かのう", "かの" },
                                               female: new[] { "かのう", "かの" })},
                      { CopulaType.DeGozaru, new(male: new[] { "でござるか" },
                                               female: new[] { "でございますか" })},
                      { CopulaType.Ssu,      new(male: new[] { "ッスか" },
                                               female: new[] { "かにゃん", "かニャ" })},
                }
            },
            {
                "kimi",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "貴方" },
                                               female: new[] { "貴方" })},
                      { CopulaType.Daze,     new(male: new[] { "お前" },
                                               female: new[] { "お前" })},
                      { CopulaType.Dayo,     new(male: new[] { "君" },
                                               female: new[] { "君" })},
                      { CopulaType.Da,       new(male: new[] { "君" },
                                               female: new[] { "君" })},
                      { CopulaType.Ja,       new(male: new[] { "お主" },
                                               female: new[] { "お主" })},
                      { CopulaType.DeGozaru, new(male: new[] { "そこもと" },
                                               female: new[] { "そなた様" })},
                      { CopulaType.Ssu,      new(male: new[] { "アンタ" },
                                               female: new[] { "あにゃた" })},
                }
            },
            {
                "ru",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "ます", "ますよ" },
                                               female: new[] { "ますわ", "ますの" })},
                      { CopulaType.Daze,     new(male: new[] { "るぜ", "るぞ" },
                                               female: new[] { "るわ", "るよ" })},
                      { CopulaType.Dayo,     new(male: new[] { "るよ", "るね" },
                                               female: new[] { "るの", "るわ" })},
                      { CopulaType.Da,       new(male: new[] { "る…", "るが…" },
                                               female: new[] { "る…", "るわ…" })},
                      { CopulaType.Ja,       new(male: new[] { "るぞよ", "るぞ" },
                                               female: new[] { "るぞよ", "るぞ" })},
                      { CopulaType.DeGozaru, new(male: new[] { "るでござる", "るでござるよ" },
                                               female: new[] { "るのでございます" })},
                      { CopulaType.Ssu,      new(male: new[] { "るッス" },
                                               female: new[] { "るのニャ", "るにゃん" })},
                }
            },
            {
                "tanomu",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "お願いします", "頼みます" },
                                               female: new[] { "お願いしますわ", "頼みますわ" })},
                      { CopulaType.Daze,     new(male: new[] { "頼む", "頼むな" },
                                               female: new[] { "頼むよ", "頼む" })},
                      { CopulaType.Dayo,     new(male: new[] { "頼むね", "頼むよ" },
                                               female: new[] { "頼むわ", "頼むね" })},
                      { CopulaType.Da,       new(male: new[] { "頼む…", "頼むぞ…" },
                                               female: new[] { "頼むわ…", "頼むよ…" })},
                      { CopulaType.Ja,       new(male: new[] { "頼むぞよ" },
                                               female: new[] { "頼むぞよ" })},
                      { CopulaType.DeGozaru, new(male: new[] { "頼み申す", "頼むでござる" },
                                               female: new[] { "お頼み申し上げます" })},
                      { CopulaType.Ssu,      new(male: new[] { "頼むッス" },
                                               female: new[] { "おねがいにゃ", "おねがいニャン" })},
                }
            },
            {
                "ore",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "私" },
                                               female: new[] { "私" })},
                      { CopulaType.Daze,     new(male: new[] { "俺" },
                                               female: new[] { "あたし" })},
                      { CopulaType.Dayo,     new(male: new[] { "僕" },
                                               female: new[] { "わたし" })},
                      { CopulaType.Da,       new(male: new[] { "自分" },
                                               female: new[] { "自分" })},
                      { CopulaType.Ja,       new(male: new[] { "麻呂" },
                                               female: new[] { "わらわ" })},
                      { CopulaType.DeGozaru, new(male: new[] { "拙者" },
                                               female: new[] { "手前" })},
                      { CopulaType.Ssu,      new(male: new[] { "あっし" },
                                               female: new[] { "みゅー" })},
                }
            },
            {
                "ga",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "ですが", "ですけど" },
                                               female: new[] { "ですが", "ですけど" })},
                      { CopulaType.Daze,     new(male: new[] { "が", "がな" },
                                               female: new[] { "が" })},
                      { CopulaType.Dayo,     new(male: new[] { "けど", "が" },
                                               female: new[] { "が", "けど" })},
                      { CopulaType.Da,       new(male: new[] { "が…", "けど…" },
                                               female: new[] { "が…", "けど…" })},
                      { CopulaType.Ja,       new(male: new[] { "であるが" },
                                               female: new[] { "であるが" })},
                      { CopulaType.DeGozaru, new(male: new[] { "でござるが" },
                                               female: new[] { "でございますが" })},
                      { CopulaType.Ssu,      new(male: new[] { "ッスけど", "ッスが" },
                                               female: new[] { "ニャけど", "にゃが" })},
                }
            },
            {
                "dana",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "ですね" },
                                               female: new[] { "ですわね", "ですね" })},
                      { CopulaType.Daze,     new(male: new[] { "だな" },
                                               female: new[] { "だね", "ね" })},
                      { CopulaType.Dayo,     new(male: new[] { "だね" },
                                               female: new[] { "ね" })},
                      { CopulaType.Da,       new(male: new[] { "だな…" },
                                               female: new[] { "だね…", "ね…" })},
                      { CopulaType.Ja,       new(male: new[] { "であるな" },
                                               female: new[] { "であるな" })},
                      { CopulaType.DeGozaru, new(male: new[] { "でござるな" },
                                               female: new[] { "でございますね" })},
                      { CopulaType.Ssu,      new(male: new[] { "ッスね" },
                                               female: new[] { "にゃ", "みゃ" })},
                }
            },
            {
                "kure",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "ください", "くださいよ" },
                                               female: new[] { "くださいな", "ください" })},
                      { CopulaType.Daze,     new(male: new[] { "くれ", "くれよ" },
                                               female: new[] { "くれ", "よ" })},
                      { CopulaType.Dayo,     new(male: new[] { "ね", "よ" },
                                               female: new[] { "ね", "ね" })},
                      { CopulaType.Da,       new(male: new[] { "くれ…", "…" },
                                               female: new[] { "よ…", "…" })},
                      { CopulaType.Ja,       new(male: new[] { "つかわせ", "たもれ" },
                                               female: new[] { "つかわせ", "たもれ" })},
                      { CopulaType.DeGozaru, new(male: new[] { "頂きたいでござる" },
                                               female: new[] { "くださいませ" })},
                      { CopulaType.Ssu,      new(male: new[] { "くれッス" },
                                               female: new[] { "にゃ", "みゃ" })},
                }
            },
            {
                "daro",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "でしょう" },
                                               female: new[] { "でしょう" })},
                      { CopulaType.Daze,     new(male: new[] { "だろ" },
                                               female: new[] { "だろうね" })},
                      { CopulaType.Dayo,     new(male: new[] { "だろうね" },
                                               female: new[] { "でしょ" })},
                      { CopulaType.Da,       new(male: new[] { "だろ…" },
                                               female: new[] { "でしょ…" })},
                      { CopulaType.Ja,       new(male: new[] { "であろう" },
                                               female: new[] { "であろうな" })},
                      { CopulaType.DeGozaru, new(male: new[] { "でござろうな" },
                                               female: new[] { "でございましょう" })},
                      { CopulaType.Ssu,      new(male: new[] { "ッスね" },
                                               female: new[] { "にゃ", "みゃ" })},
                }
            },
            {
                "yo",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "ですよ", "です" },
                                               female: new[] { "ですよ", "です" })},
                      { CopulaType.Daze,     new(male: new[] { "ぜ", "ぞ" },
                                               female: new[] { "わ", "よ" })},
                      { CopulaType.Dayo,     new(male: new[] { "よ", "ぞ" },
                                               female: new[] { "わよ", "わ" })},
                      { CopulaType.Da,       new(male: new[] { "…", "ぞ…" },
                                               female: new[] { "わ…", "…" })},
                      { CopulaType.Ja,       new(male: new[] { "であろう", "でおじゃる" },
                                               female: new[] { "であろうぞ", "でおじゃる" })},
                      { CopulaType.DeGozaru, new(male: new[] { "でござろう" },
                                               female: new[] { "でございますわ" })},
                      { CopulaType.Ssu,      new(male: new[] { "ッスよ", "ッス" },
                                               female: new[] { "にゃぁ", "みゃぁ" })},
                }
            },
            {
                "aru",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "あります", "ありますね" },
                                               female: new[] { "あります", "ありますわ" })},
                      { CopulaType.Daze,     new(male: new[] { "ある", "あるな" },
                                               female: new[] { "あるね", "あるよ" })},
                      { CopulaType.Dayo,     new(male: new[] { "あるね", "あるよ" },
                                               female: new[] { "あるわ", "あるわね" })},
                      { CopulaType.Da,       new(male: new[] { "ある…", "あるぞ…" },
                                               female: new[] { "あるわ…" })},
                      { CopulaType.Ja,       new(male: new[] { "あろう", "おじゃる" },
                                               female: new[] { "あろう", "おじゃる" })},
                      { CopulaType.DeGozaru, new(male: new[] { "あるでござる", "あるでござるな" },
                                               female: new[] { "ござます" })},
                      { CopulaType.Ssu,      new(male: new[] { "あるッスよ", "あるッス" },
                                               female: new[] { "あにゅ", "あみぅ" })},
                }
            },
            {
                "u",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "います", "いますよ" },
                                               female: new[] { "いますわ", "います" })},
                      { CopulaType.Daze,     new(male: new[] { "うぜ", "うぞ" },
                                               female: new[] { "うわ", "うよ" })},
                      { CopulaType.Dayo,     new(male: new[] { "うよ", "う" },
                                               female: new[] { "うわ", "う" })},
                      { CopulaType.Da,       new(male: new[] { "う…", "うぞ…" },
                                               female: new[] { "うわ…", "う…" })},
                      { CopulaType.Ja,       new(male: new[] { "うぞよ", "うぞ" },
                                               female: new[] { "うぞよ", "うぞ" })},
                      { CopulaType.DeGozaru, new(male: new[] { "うでござる", "うでござるよ" },
                                               female: new[] { "うでございます" })},
                      { CopulaType.Ssu,      new(male: new[] { "うッスよ", "うッス" },
                                               female: new[] { "うにぁ", "うみぁ" })},
                }
            },
            {
                "na",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "ですね", "です" },
                                               female: new[] { "ですわ", "ですね" })},
                      { CopulaType.Daze,     new(male: new[] { "ぜ", "な" },
                                               female: new[] { "ね", "な" })},
                      { CopulaType.Dayo,     new(male: new[] { "ね", "なぁ" },
                                               female: new[] { "わ", "わね" })},
                      { CopulaType.Da,       new(male: new[] { "…", "な…" },
                                               female: new[] { "…", "わ…" })},
                      { CopulaType.Ja,       new(male: new[] { "でおじゃるな", "のう" },
                                               female: new[] { "でおじゃるな", "のう" })},
                      { CopulaType.DeGozaru, new(male: new[] { "でござるな" },
                                               female: new[] { "でございますわ" })},
                      { CopulaType.Ssu,      new(male: new[] { "ッスね", "ッス" },
                                               female: new[] { "ニァ", "ミァ" })},
                }
            },
            {
                "ta",
                new() {
                      { CopulaType.Desu,     new(male: new[] { "ました", "ましたね" },
                                               female: new[] { "ました", "ましたわ" })},
                      { CopulaType.Daze,     new(male: new[] { "た", "たな" },
                                               female: new[] { "たね", "たよ" })},
                      { CopulaType.Dayo,     new(male: new[] { "たね", "たよ" },
                                               female: new[] { "たよ", "たね" })},
                      { CopulaType.Da,       new(male: new[] { "た…", "たぞ…" },
                                               female: new[] { "たわ…" })},
                      { CopulaType.Ja,       new(male: new[] { "たぞよ", "たぞな" },
                                               female: new[] { "たぞよ" })},
                      { CopulaType.DeGozaru, new(male: new[] { "たでござる" },
                                               female: new[] { "ましてございます" })},
                      { CopulaType.Ssu,      new(male: new[] { "たッスよ", "たッス" },
                                               female: new[] { "たにゃぁ", "たみゃぁ" })},
                }
            },
        };

#pragma warning restore format

        [LocaleFunction("yoro")]
        public static string BuiltIn_yoro(object? obj, int? mark = null) => PrintCopula("yoro", obj, mark);
        [LocaleFunction("dozo")]
        public static string BuiltIn_dozo(object? obj, int? mark = null) => PrintCopula("dozo", obj, mark);
        [LocaleFunction("thanks")]
        public static string BuiltIn_thanks(object? obj, int? mark = null) => PrintCopula("thanks", obj, mark);
        [LocaleFunction("rob")]
        public static string BuiltIn_rob(object? obj, int? mark = null) => PrintCopula("rob", obj, mark);
        [LocaleFunction("ka")]
        public static string BuiltIn_ka(object? obj, int? mark = null) => PrintCopula("ka", obj, mark);
        [LocaleFunction("da")]
        public static string BuiltIn_da(object? obj, int? mark = null) => PrintCopula("da", obj, mark);
        [LocaleFunction("noda")]
        public static string BuiltIn_noda(object? obj, int? mark = null) => PrintCopula("noda", obj, mark);
        [LocaleFunction("noka")]
        public static string BuiltIn_noka(object? obj, int? mark = null) => PrintCopula("noka", obj, mark);
        [LocaleFunction("kana")]
        public static string BuiltIn_kana(object? obj, int? mark = null) => PrintCopula("kana", obj, mark);
        [LocaleFunction("kimi")]
        public static string BuiltIn_kimi(object? obj, int? mark = null) => PrintCopula("kimi", obj, mark);
        [LocaleFunction("ru")]
        public static string BuiltIn_ru(object? obj, int? mark = null) => PrintCopula("ru", obj, mark);
        [LocaleFunction("tanomu")]
        public static string BuiltIn_tanomu(object? obj, int? mark = null) => PrintCopula("tanomu", obj, mark);
        [LocaleFunction("ore")]
        public static string BuiltIn_ore(object? obj, int? mark = null) => PrintCopula("ore", obj, mark);
        [LocaleFunction("ga")]
        public static string BuiltIn_ga(object? obj, int? mark = null) => PrintCopula("ga", obj, mark);
        [LocaleFunction("dana")]
        public static string BuiltIn_dana(object? obj, int? mark = null) => PrintCopula("dana", obj, mark);
        [LocaleFunction("kure")]
        public static string BuiltIn_kure(object? obj, int? mark = null) => PrintCopula("kure", obj, mark);
        [LocaleFunction("daro")]
        public static string BuiltIn_daro(object? obj, int? mark = null) => PrintCopula("daro", obj, mark);
        [LocaleFunction("yo")]
        public static string BuiltIn_yo(object? obj, int? mark = null) => PrintCopula("yo", obj, mark);
        [LocaleFunction("aru")]
        public static string BuiltIn_aru(object? obj, int? mark = null) => PrintCopula("aru", obj, mark);
        [LocaleFunction("u")]
        public static string BuiltIn_u(object? obj, int? mark = null) => PrintCopula("u", obj, mark);
        [LocaleFunction("na")]
        public static string BuiltIn_na(object? obj, int? mark = null) => PrintCopula("na", obj, mark);
        [LocaleFunction("ta")]
        public static string BuiltIn_ta(object? obj, int? mark = null) => PrintCopula("ta", obj, mark);

    }
}
