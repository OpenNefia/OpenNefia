using System.Reflection;

namespace OpenNefia.Core
{
    public static class Engine
    {
        public const string NameBase = "OpenNefia.NET";
        public const string Title = "OpenNefia";

        public static Version Version { get => Assembly.GetExecutingAssembly().GetName().Version!; }
    }
}
