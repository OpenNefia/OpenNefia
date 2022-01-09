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
    public interface IRandomNameGenerator
    {
        void Initialize();
        string GenerateRandomName();
    }

    public class RandomNameGenerator : IRandomNameGenerator
    {
        [Dependency] private readonly IRandom _random = default!;
        [Dependency] private readonly IResourceManager _resourceManager = default!;
        [Dependency] private readonly ILocalizationManager _localeMan = default!;

        private record NameData(string Base, string OptionalSuffix);

        private readonly List<NameData> _allNameData = new();

        public void Initialize()
        {
            _localeMan.OnLanguageSwitched += LoadNameFiles;
        }

        private void LoadNameFiles(PrototypeId<LanguagePrototype> newLanguage)
        {
            _allNameData.Clear();

            var csvPaths = _resourceManager
                .ContentFindFiles(new ResourcePath("/Text/RandomNames") / (string)newLanguage)
                .Where(path => path.Extension == "csv");

            foreach (var csvPath in csvPaths)
            {
                LoadNameFile(csvPath);
            }
        }

        private void LoadNameFile(ResourcePath csvPath)
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
                        var choice1 = fields[0];
                        var choice2 = fields[1];
                        var data = new NameData(choice1, choice2);

                        _allNameData.Add(data);
                    }
                }
            }
        }

        private bool TryGetRandomName([NotNullWhen(true)] out string? name)
        {
            name = null;
            var data = _random.Pick(_allNameData);
            var result = data.Base;

            if (_localeMan.Language == LanguagePrototypeOf.Japanese && _random.OneIn(8))
                result += "ー";

            if (_random.OneIn(5))
                result += data.OptionalSuffix;

            if (result.Length < 4)
                return false;

            if (result.Length < 6 && _random.OneIn(3))
                return false;

            if (result.Length < 8 && _random.OneIn(2))
                return false;

            if (_localeMan.Language == LanguagePrototypeOf.Japanese)
            {
                if (result.StartsWith("ー") || result.Contains("﻿ーッ"))
                    return false;
            }

            name = RandomTextHelpers.CapitalizeTitleText(result, _localeMan);
            return true;
        }

        public string GenerateRandomName()
        {
            string? name;

            while (!TryGetRandomName(out name)) { }

            return name;
        }
    }
}
