using Love;
using Microsoft.VisualBasic.FileIO;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Stats;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;

namespace OpenNefia.Content.Aliases
{
    public enum AliasType
    {
        Chara,
        Weapon,
        Party,
        LivingWeapon
    }

    public interface IRandomAliasGenerator
    {
        void Initialize();
        string GenerateRandomAlias(AliasType type);
    }

    public class RandomAliasGenerator : IRandomAliasGenerator
    {
        private enum TitleDataCategory
        {
            Royal,         // 王
            Tool,          // 具
            Shape,         // 形
            Time,          // 時
            Beast,         // 獣
            Feeling,       // 情
            Color,         // 色
            God,           // 神
            Person,        // 人
            World,         // 世界
            Star,          // 星
            Rock,          // 石
            Element,       // 属
            Attribute,     // 属性
            Race,          // 族
            Body,          // 体
            Movement,      // 動
            Animal,        // 動物
            Regular,       // 普
            Object,        // 物
            GeneralPurpose // 万能
        }

        private static TitleDataCategory ParseTitleDataType(string str)
        {
            switch (str)
            {
                case "王": return TitleDataCategory.Royal;
                case "具": return TitleDataCategory.Tool;
                case "形": return TitleDataCategory.Shape;
                case "時": return TitleDataCategory.Time;
                case "獣": return TitleDataCategory.Beast;
                case "情": return TitleDataCategory.Feeling;
                case "色": return TitleDataCategory.Color;
                case "神": return TitleDataCategory.God;
                case "人": return TitleDataCategory.Person;
                case "世界": return TitleDataCategory.World;
                case "星": return TitleDataCategory.Star;
                case "石": return TitleDataCategory.Rock;
                case "属": return TitleDataCategory.Element;
                case "属性": return TitleDataCategory.Attribute;
                case "族": return TitleDataCategory.Race;
                case "体": return TitleDataCategory.Body;
                case "動": return TitleDataCategory.Movement;
                case "動物": return TitleDataCategory.Animal;
                case "普": return TitleDataCategory.Regular;
                case "物": return TitleDataCategory.Object;
                case "万能":
                default:
                    return TitleDataCategory.GeneralPurpose;
            }
        }

        private class TitleData
        {
            public TitleDataCategory Category { get; }
            public List<string> Choices { get; }

            public TitleData(TitleDataCategory type, List<string> choices)
            {
                Category = type;
                Choices = choices;
            }
        }

        [Dependency] private readonly IRandom _random = default!;
        [Dependency] private readonly IResourceManager _resourceManager = default!;
        [Dependency] private readonly ILocalizationManager _localizationManager = default!;

        private readonly List<TitleData> _allTitleData = new();

        public void Initialize()
        {
            _localizationManager.OnLanguageSwitched += LoadAliasFiles;
        }

        private void LoadAliasFiles(PrototypeId<LanguagePrototype> newLanguage)
        {
            _allTitleData.Clear();

            var csvPaths = _resourceManager
                .ContentFindFiles(new ResourcePath("/Aliases") / ((string)newLanguage))
                .Where(path => path.Extension == "csv");

            foreach (var csvPath in csvPaths)
            {
                LoadFile(csvPath);
            }
        }

        private void LoadFile(ResourcePath csvPath)
        {
            using (var stream = _resourceManager.ContentFileReadText(csvPath))
            {
                using (var csvParser = new TextFieldParser(stream))
                {
                    csvParser.SetDelimiters(new string[] { "," });

                    while (!csvParser.EndOfData)
                    {
                        // Read current line fields, pointer moves to the next line.
                        var fields = csvParser.ReadFields()!;
                        var type = ParseTitleDataType(fields[14]);
                        var choices = fields.Take(13).ToList();
                        var data = new TitleData(type, choices);

                        _allTitleData.Add(data);
                    }
                }
            }
        }

        private (TitleData data, int column, string choice) RandomTitleAndChoice()
        {
            TitleData data;
            int column;
            string choice;

            do
            {
                data = _random.Pick(_allTitleData);
                column = _random.Next(data.Choices.Count);
                choice = data.Choices[column];
            }
            while (string.IsNullOrEmpty(choice));

            return (data, column, choice);
        }

        private string CapitalizeTitleText(string text)
        {
            if (_localizationManager.Language != LanguagePrototypeOf.English || string.IsNullOrEmpty(text))
                return text;

            if (text[0] == '*')
            {
                if (text.Length == 1)
                    return text;

                text = text.Substring(1);
            }

            return text.FirstCharToUpper();
        }

        private const int MaxTitleLength = 28;

        private readonly string[] PartyTitleSuffixesJP =
        {
            "団",
            "チーム",
            "パーティー",
            "の集い",
            "の軍",
            "アーミー",
            "隊",
            "の一家",
            "軍",
            "の隊",
            "の団",
        };

        private readonly string[] PartyTitlePrefixesEN =
        {
            "The army of",
            "The party of",
            "The house of",
            "Clan",
        };

        private readonly string[] PartyTitleSuffixesEN =
        {
            "Clan",
            "Party",
            "Band",
            "Gangs",
            "Gathering",
            "House",
            "Army",
        };

        private bool TryGetRandomTitle(AliasType type, [NotNullWhen(true)] out string? title)
        {
            title = null;
            var (data, column, result) = RandomTitleAndChoice();

            if ((type == AliasType.Weapon || type == AliasType.LivingWeapon) && data.Category == TitleDataCategory.Tool)
                return false;

            var skip = false;

            if (_localizationManager.Language == LanguagePrototypeOf.English)
            {
                if (column == 0 || column == 1)
                {
                    if (_random.OneIn(6))
                    {
                        result += " of";
                    }
                    else if (_random.OneIn(6))
                    {
                        result = "the " + result;
                        skip = true;
                    }
                }

                if (!skip)
                    result += " ";

                result = CapitalizeTitleText(result);
            }

            if (!skip)
            {
                string? found = null;

                for (int i = 0; i < 100; i++)
                {
                    var data2 = _random.Pick(_allTitleData);

                    if (data != data2 && data.Category != data2.Category
                        && data.Category != TitleDataCategory.GeneralPurpose
                        && data2.Category != TitleDataCategory.GeneralPurpose)
                    {
                        int newColumn;
                        if (column < 10)
                        {
                            newColumn = _random.Next(2);
                        }
                        else
                        {
                            newColumn = _random.Next(2) + 10;
                        }

                        var check = data2.Choices[newColumn];
                        if (!string.IsNullOrEmpty(check))
                        {
                            found = check;
                            break;
                        }
                    }
                }

                if (found == null)
                    return false;

                found = CapitalizeTitleText(found);

                result += found;

                // NOTE: Not using GetWideLength() here.
                if (result.Length >= MaxTitleLength)
                    return false;
            }

            if (type == AliasType.Party)
            {
                if (_localizationManager.Language == LanguagePrototypeOf.Japanese)
                {
                    var suffix = _random.Pick(PartyTitleSuffixesJP);
                    result += suffix;
                }
                else if (_random.OneIn(2))
                {
                    var prefix = _random.Pick(PartyTitlePrefixesEN);
                    result = prefix + " " + result;
                }
                else
                {
                    var suffix = _random.Pick(PartyTitleSuffixesEN);
                    result += " " + suffix;
                }
            }

            title = result;
            return true;
        }

        public string GenerateRandomAlias(AliasType type)
        {
            string? title = null;

            while (!TryGetRandomTitle(type, out title)) { }

            return title;
        }
    }
}
