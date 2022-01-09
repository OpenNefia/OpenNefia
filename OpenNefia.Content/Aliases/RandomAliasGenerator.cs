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
        private enum TitleDataType
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

        private static TitleDataType ParseTitleDataType(string str)
        {
            switch (str)
            {
                case "王":   return TitleDataType.Royal;
                case "具":   return TitleDataType.Tool;
                case "形":   return TitleDataType.Shape;
                case "時":   return TitleDataType.Time;
                case "獣":   return TitleDataType.Beast;
                case "情":   return TitleDataType.Feeling;
                case "色":   return TitleDataType.Color;
                case "神":   return TitleDataType.God;
                case "人":   return TitleDataType.Person;
                case "世界": return TitleDataType.World;
                case "星":   return TitleDataType.Star;
                case "石":   return TitleDataType.Rock;
                case "属":   return TitleDataType.Element;
                case "属性": return TitleDataType.Attribute;
                case "族":   return TitleDataType.Race;
                case "体":   return TitleDataType.Body;
                case "動":   return TitleDataType.Movement;
                case "動物": return TitleDataType.Animal;
                case "普":   return TitleDataType.Regular;
                case "物":   return TitleDataType.Object;
                case "万能":
                default:
                    return TitleDataType.GeneralPurpose;
            }
        }

        private class TitleData
        {
            public TitleDataType Type { get; }
            public List<string> Choices { get; }

            public TitleData(TitleDataType type, List<string> choices)
            {
                Type = type;
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

        public string GenerateRandomAlias(AliasType type)
        {
            return "";
        }
    }
}
