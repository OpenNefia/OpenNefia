using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static OpenNefia.EditorExtension.InsertDependencyCommandPackage;

namespace OpenNefia.EditorExtension 
{
    public static class InsertDependencyUtility
    {
        private static Regex EntitySystemRegex = new Regex("^(I)?(.*)System$");
        private static Regex ManagerRegex = new Regex("^(I)?(.*)Manager$");
        private static Regex GeneralTypeRegex = new Regex("^(I)?(.*)$");

        public static string GetDefaultPropertyNameForType(string shortName)
        {
            var match = EntitySystemRegex.Match(shortName);
            if (match.Success)
            {
                // ISkillSystem -> _skills
                var noun = match.Groups[2].Value.FirstCharToLowerCase();
                return $"_{noun.Pluralize()}";
            }

            foreach (var regex in new Regex[] { ManagerRegex, GeneralTypeRegex })
            {
                match = regex.Match(shortName);
                if (match.Success)
                {
                    return "_" + match.Groups[2].Value.FirstCharToLowerCase();
                }
            }

            return shortName.FirstCharToLowerCase();
        }

        private static string FirstCharToLowerCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && char.IsUpper(str[0]))
                return str.Length == 1 ? char.ToLower(str[0]).ToString() : char.ToLower(str[0]) + str.Substring(1);

            return str;
        }
    }
}