using OpenNefia.Core.Locale;

namespace OpenNefia.Content.Resists
{
    public static class ResistHelpers
    {
        public const int LevelsPerGrade = 50;
        
        public static int CalculateGrade(int level)
        {
            return level / LevelsPerGrade;
        }

        public static string GetGradeText(int grade)
        {
            grade = Math.Clamp(grade, 0, 6);
            return Loc.GetString($"Elona.CharaInfo.SkillsList.Resist.Grade.{grade}");
        }
    }

    public static class ResistGrades
    {
        public const int Minimum = 3;
        public const int Strong = 7;
        public const int Immune = 10;
    }
}