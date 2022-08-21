using Microsoft.VisualBasic.FileIO;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.RandomText
{
    public enum AliasType
    {
        Chara,
        Item,
        Party
    }

    public interface IRandomAliasGenerator
    {
        void Initialize();
        string GenerateRandomAlias(AliasType type);
    }

    public class RandomAliasGenerator : IRandomAliasGenerator
    {
        private enum AliasDataCategory
        {
            Unknown,

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

        private static AliasDataCategory ParseAliasDataType(string str)
        {
            switch (str)
            {
                case "王": return AliasDataCategory.Royal;
                case "具": return AliasDataCategory.Tool;
                case "形": return AliasDataCategory.Shape;
                case "時": return AliasDataCategory.Time;
                case "獣": return AliasDataCategory.Beast;
                case "情": return AliasDataCategory.Feeling;
                case "色": return AliasDataCategory.Color;
                case "神": return AliasDataCategory.God;
                case "人": return AliasDataCategory.Person;
                case "世界": return AliasDataCategory.World;
                case "星": return AliasDataCategory.Star;
                case "石": return AliasDataCategory.Rock;
                case "属": return AliasDataCategory.Element;
                case "属性": return AliasDataCategory.Attribute;
                case "族": return AliasDataCategory.Race;
                case "体": return AliasDataCategory.Body;
                case "動": return AliasDataCategory.Movement;
                case "動物": return AliasDataCategory.Animal;
                case "普": return AliasDataCategory.Regular;
                case "物": return AliasDataCategory.Object;
                case "万能": return AliasDataCategory.GeneralPurpose;

                default: return AliasDataCategory.Unknown;
            }
        }

        private record AliasData(AliasDataCategory Category, IReadOnlyList<string> Choices);

        [Dependency] private readonly IRandom _random = default!;
        [Dependency] private readonly IResourceManager _resourceManager = default!;
        [Dependency] private readonly ILocalizationManager _localeMan = default!;

        private readonly List<AliasData> _allAliasData = new();

        public void Initialize()
        {
            _localeMan.OnLanguageSwitched += LoadAliasFiles;
        }

        private void LoadAliasFiles(PrototypeId<LanguagePrototype> newLanguage)
        {
            _allAliasData.Clear();

            var csvPaths = _resourceManager
                .ContentFindFiles(new ResourcePath("/Text/RandomAliases") / (string)newLanguage)
                .Where(path => path.Extension == "csv");

            foreach (var csvPath in csvPaths)
            {
                LoadAliasFile(csvPath);
            }
        }

        private void LoadAliasFile(ResourcePath csvPath)
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
                        var type = ParseAliasDataType(fields[14]);
                        var choices = fields.Take(13).ToList();
                        var data = new AliasData(type, choices);

                        _allAliasData.Add(data);
                    }
                }
            }
        }

        private (AliasData data, int column, string choice) RandomAliasAndChoice()
        {
            AliasData data;
            int column;
            string choice;

            do
            {
                data = _random.Pick(_allAliasData);
                column = _random.Next(data.Choices.Count);
                choice = data.Choices[column];
            }
            while (string.IsNullOrEmpty(choice));

            return (data, column, choice);
        }

        private const int MaxTitleLength = 28;

        private readonly string[] PartyAliasSuffixesJP =
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

        private readonly string[] PartyAliasPrefixesEN =
        {
            "The army of",
            "The party of",
            "The house of",
            "Clan",
        };

        private readonly string[] PartyAliasSuffixesEN =
        {
            "Clan",
            "Party",
            "Band",
            "Gangs",
            "Gathering",
            "House",
            "Army",
        };

        private bool TryGetRandomTitle(AliasType type, [NotNullWhen(true)] out string? alias)
        {
            if (_allAliasData.Count == 0)
            {
                alias = "???";
                return true;
            }    

            alias = null;
            var (data, column, result) = RandomAliasAndChoice();

            if (type == AliasType.Item && data.Category == AliasDataCategory.Tool)
                return false;

            var noSecondPart = false;

            if (_localeMan.Language == LanguagePrototypeOf.Japanese)
            {
                while (true)
                {
                    if (column == 10 || column == 11)
                    {
                        if (_random.OneIn(5))
                        {
                            column = 0;
                            if (_random.OneIn(2))
                            {
                                result += "の";
                            }
                            break;
                        }

                        switch (_random.Next(5))
                        {
                            case 0:
                                result += "・オブ・";
                                break;
                            case 1:
                                result = "ザ・" + result;
                                noSecondPart = true;
                                break;
                            case 2:
                                result += "・";
                                break;
                        }
                    }

                    if (column == 0 || column == 1)
                    {
                        result += "の";
                        if (_random.OneIn(10))
                        {
                            column = 10;
                        }
                    }

                    break;
                }
            }
            else if (_localeMan.Language == LanguagePrototypeOf.English)
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
                        noSecondPart = true;
                    }
                }

                if (!noSecondPart)
                    result += " ";

                result = RandomTextHelpers.CapitalizeTitleText(result, _localeMan);
            }

            if (!noSecondPart)
            {
                string? found = null;

                for (int i = 0; i < 100; i++)
                {
                    var data2 = _random.Pick(_allAliasData);

                    if (data != data2 && data.Category != data2.Category
                        && data.Category != AliasDataCategory.GeneralPurpose
                        && data2.Category != AliasDataCategory.GeneralPurpose)
                    {
                        if (column < 10)
                        {
                            column = _random.Next(2);
                        }
                        else
                        {
                            column = _random.Next(2) + 10;
                        }

                        var check = data2.Choices[column];
                        if (!string.IsNullOrEmpty(check))
                        {
                            found = check;
                            break;
                        }
                    }
                }

                if (found == null)
                    return false;

                found = RandomTextHelpers.CapitalizeTitleText(found, _localeMan);

                result += found;

                // NOTE: Not using GetWideLength() here.
                if (result.Length >= MaxTitleLength)
                    return false;
            }

            if (type == AliasType.Party)
            {
                if (_localeMan.Language == LanguagePrototypeOf.Japanese)
                {
                    var suffix = _random.Pick(PartyAliasSuffixesJP);
                    result += suffix;
                }
                else if (_random.OneIn(2))
                {
                    var prefix = _random.Pick(PartyAliasPrefixesEN);
                    result = prefix + " " + result;
                }
                else
                {
                    var suffix = _random.Pick(PartyAliasSuffixesEN);
                    result += " " + suffix;
                }
            }

            alias = result;
            return true;
        }

        public string GenerateRandomAlias(AliasType type)
        {
            string? alias;

            while (!TryGetRandomTitle(type, out alias)) { }

            return alias;
        }
    }
}
