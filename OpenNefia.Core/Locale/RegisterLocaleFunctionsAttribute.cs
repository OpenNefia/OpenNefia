using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Locale
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterLocaleFunctionsAttribute : Attribute
    {
        public PrototypeId<LanguagePrototype>? Language { get; }
        public string? Module { get; }

        public RegisterLocaleFunctionsAttribute(string? module = null, string? language = null)
        {
            Language = language != null ? new(language) : null;
            Module = module;
        }
    }
}